﻿using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Web;
using TownBulletin.Extensions;
using TownBulletin.Models;
using TownBulletin.Services;

namespace TownBulletin.Components
{
    public partial class TestingCenter
    {
        #region Services Injection

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;

        [Inject]
        private IDbContextFactory<TownBulletinDbContext> DbContextFactory { get; set; } = null!;

        [Inject]
        private ModuleService ModuleService { get; set; } = null!;

        [Inject]
        private MessagingService MessagingService { get; set; } = null!;

        #endregion

        #region Private Variables

        private IEnumerable<EventTest> EventTests { get; set; } = Array.Empty<EventTest>();
        private Dictionary<string, string>? EventTestNames { get; set; }
        private Dictionary<string, IEnumerable<string>>? CategoryEventTests { get; set; }

        private EventTest CurrentEventTest { get; set; } = new();
        private bool CurrentEventTestIsValid = false;
        private bool CurrentEventTestIsDirty = false;

        private ModalPrompt ModalPromptReference = null!;

        #endregion

        #region Lifecycle Methods

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("initializeMenuActions", DotNetObjectReference.Create(this));

                await UpdatePageState();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        #endregion

        #region UI Events

        private async Task UpdateProperty(string propertyName, object? value)
        {
            if (propertyName == nameof(EventTest.Event) && value is string eventString)
            {
                if (string.IsNullOrWhiteSpace(eventString))
                    return;

                string currentEventBlankArgsJson = GetBlankEventArgsJson(CurrentEventTest.Event);

                if (string.IsNullOrWhiteSpace(CurrentEventTest.JsonData) || CurrentEventTest.JsonData.Equals(currentEventBlankArgsJson))
                    CurrentEventTest.JsonData = GetBlankEventArgsJson(eventString);                
            }

            if (propertyName == nameof(EventTest.JsonData) && value is string jsonDataString && string.IsNullOrWhiteSpace(jsonDataString))
                value = null;

            typeof(EventTest).GetProperty(propertyName)!.SetValue(CurrentEventTest, value);

            await UpdatePageState();
        }

        private async Task CreateEventTest()
        {
            async void createAction()
            {
                await SetCurrentEventTest(new EventTest());
            }

            if (CurrentEventTestIsDirty)
                await ModalPromptReference.ShowModalPrompt(new()
                {
                    Title = "WARNING: Losing changes!",
                    Message = $"Are you sure you want to create a new event test and dismiss the unsaved changes?",
                    CancelChoice = "Cancel",
                    Choice = "Yes",
                    ChoiceColour = "danger",
                    ChoiceAction = createAction
                });
            else
                createAction();
        }

        private async Task LoadEventTest(string eventTestId)
        {
            if (!int.TryParse(eventTestId, out int id) && id == CurrentEventTest.Id)
                return;

            EventTest? eventTest = EventTests.FirstOrDefault(eventTests => eventTests.Id == id);

            if (eventTest == null)
                return;            

            async void loadAction()
            {
                using TownBulletinDbContext townBulletinDbContext = await DbContextFactory.CreateDbContextAsync();

                await SetCurrentEventTest((EventTest)townBulletinDbContext.Entry(eventTest).CurrentValues.Clone().ToObject());
            }

            if (CurrentEventTestIsDirty)
                await ModalPromptReference.ShowModalPrompt(new()
                {
                    Title = "WARNING: Losing changes!",
                    Message = $"Are you sure you want to edit the event test \"{eventTest.Name}\" and dismiss the unsaved changes?",
                    CancelChoice = "Cancel",
                    Choice = "Yes",
                    ChoiceColour = "danger",
                    ChoiceAction = loadAction
                });
            else
                loadAction();
        }

        private async Task SaveEventTest()
        {
            //TODO: malformed json error.
            using TownBulletinDbContext townBulletinDbContext = await DbContextFactory.CreateDbContextAsync();

            if (townBulletinDbContext.EventTests.Any(module => module.Id == CurrentEventTest.Id))
            {
                townBulletinDbContext.EventTests.Update(CurrentEventTest);
                await townBulletinDbContext.SaveChangesAsync();

                await ModalPromptReference.ShowModalPrompt(new()
                {
                    Title = "Module updated!",
                    Message = $"Successfully updated the event test \"{CurrentEventTest.Name}\".",
                    CancelChoice = "Dismiss"
                });
            }
            else
            {
                townBulletinDbContext.EventTests.Add(CurrentEventTest);
                await townBulletinDbContext.SaveChangesAsync();

                await ModalPromptReference.ShowModalPrompt(new()
                {
                    Title = "Module added!",
                    Message = $"Successfully added the event test \"{CurrentEventTest.Name}\".",
                    CancelChoice = "Dismiss"
                });
            }

            await UpdatePageState();
        }

        private async Task DeleteEventTest(string eventTestId)
        {
            if (!int.TryParse(eventTestId, out int id))
                return;

            EventTest? eventTest = EventTests.FirstOrDefault(eventTest => eventTest.Id == id);

            if (eventTest == null)
                return;

            await ModalPromptReference.ShowModalPrompt(new()
            {
                Title = "WARNING: Event test deletion!",
                Message = $"Are you sure you want to delete the event test \"{eventTest.Name}\"?",
                CancelChoice = "Cancel",
                Choice = "Delete",
                ChoiceColour = "danger",
                ChoiceAction = async () =>
                {
                    if (CurrentEventTest.Id == eventTest.Id)
                        await CreateEventTest();

                    using TownBulletinDbContext townBulletinDbContext = await DbContextFactory.CreateDbContextAsync();
                    townBulletinDbContext.Remove(eventTest);
                    await townBulletinDbContext.SaveChangesAsync();

                    await MessagingService.AddMessage($"Succesfully removed the event test \"{eventTest.Name}\"!", MessageCategory.Module);

                    await ModalPromptReference.ShowModalPrompt(new()
                    {
                        Title = "Event test removed!",
                        Message = $"Successfully removed the event test \"{eventTest.Name}\".",
                        CancelChoice = "Dismiss"
                    });

                    await UpdatePageState();
                }
            });

        }

        private async Task TestEvent()
        {
            if (ModuleService.EventArgumentTypes.TryGetValue(CurrentEventTest.Event, out Type? argumentType) && argumentType != null)
            {
                object? eventargs; Newtonsoft.Json.Serialization.ErrorEventArgs? errorEventArgs = null;
                if (string.IsNullOrEmpty(CurrentEventTest.JsonData))
                    eventargs = new EventArgs();
                else
                    eventargs = JsonConvert.DeserializeObject(CurrentEventTest.JsonData, argumentType, new JsonSerializerSettings { Error = (_, eventArgs) => { errorEventArgs = eventArgs; } });

                if (eventargs != null)
                    await ModuleService.ExecuteModules(CurrentEventTest.Event, eventargs);
                else if (errorEventArgs != null)
                    await ModalPromptReference.ShowModalPrompt(new()
                    {
                        Title = "JSON data error.",
                        Message = $"The JSON data could not be parsed as \"{nameof(argumentType)}\". Please make sure the JSON data is accurate and try again.",
                        CancelChoice = "Dismiss"
                    });
            }
        }

        #endregion


        #region Helper Methods

        private async Task UpdatePageState()
        {
            CurrentEventTestIsValid =
                !string.IsNullOrWhiteSpace(CurrentEventTest.Name) &&
                !string.IsNullOrWhiteSpace(CurrentEventTest.Event);

            TownBulletinDbContext townBulletinDbContext = await DbContextFactory.CreateDbContextAsync();

            CurrentEventTestIsDirty = (CurrentEventTest.Id == null && !EventTestComparer.MemberwiseComparer.Equals(CurrentEventTest, new EventTest())) ||
                (CurrentEventTest.Id != null && !EventTestComparer.MemberwiseComparer.Equals(CurrentEventTest, townBulletinDbContext.EventTests.FirstOrDefault(eventTest => eventTest.Id == CurrentEventTest.Id)));

            EventTests = townBulletinDbContext.EventTests.ToList();
            EventTestNames = EventTests.OrderBy(eventTest => eventTest.Name).ToDictionary(eventTest => eventTest.Id.ToString()!, eventTest => eventTest.Name);
            CategoryEventTests = EventTests.OrderBy(eventTest => eventTest.Name).ThenBy(eventTest => eventTest.Event).GroupBy(module => module.Event)
                .ToDictionary(category => category.Key.Split(".").First() + " / " + category.Key.Split(".").Last(), category => category.Select(module => module.Id.ToString()!));

            await InvokeAsync(StateHasChanged);
        }

        private async Task SetCurrentEventTest(EventTest eventTest)
        {
            CurrentEventTest = eventTest;
            await UpdatePageState();
        }   

        private string GetBlankEventArgsJson(string @event)
        {
            if (ModuleService.EventArgumentTypes.TryGetValue(@event, out Type? argumentType) && argumentType != null)
            {
                return JsonConvert.SerializeObject(Activator.CreateInstance(argumentType), new JsonSerializerSettings { Formatting = Formatting.Indented });
            }
            else
                return string.Empty;
        }

        #endregion
    }
}
