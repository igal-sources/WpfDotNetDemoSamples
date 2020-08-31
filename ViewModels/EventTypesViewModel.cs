using EventTypesDemo.DataModels;
using EventTypesDemo.Helpers;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace EventTypesDemo.ViewModels
{
    public class EventItems
    {
        public ObservableCollection<EventTypes> EventTypes { get; set; }
    }

    public class EventTypesViewModel : ViewModelBase
    {
        #region Properties
        private EventItems _eventTypes;
        private EventTypes _selectedEventType;

        public EventItems EventTypesUI
        {
            get { return _eventTypes; }
            set
            {
                if (_eventTypes == value) return;

                _eventTypes = value;
                OnPropertyChanged("EventTypesUI");
            }
        }

        #endregion

        public EventTypesViewModel()
        {
            var events = DeserializeEvents();
            EventTypesUI = new EventItems
            {
                EventTypes = new ObservableCollection<EventTypes>()
            };

            RefreshEventTypeTree(events);
        }

        private EventItems DeserializeEvents()
        {
            using (StreamReader reader = new StreamReader(@"C:\CIT-Stuff\MyDevelopment\DotNetSamples\EventTypesDemo\DataModels\event-types.json"))
            {
                string jsonString = reader.ReadToEnd();
                var events = (EventItems)JsonConvert.DeserializeObject(jsonString, typeof(EventItems));

                return events;
            }
        }

        #region Commands

        private RelayCommand<object> _addCommand;

        public ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RelayCommand<object>(x => AddEventType());
                }

                return _addCommand;
            }
        }

        private RelayCommand<object> _removeCommand;

        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new RelayCommand<object>(x => RemoveEventType());
                }

                return _removeCommand;
            }
        }

        private RelayCommand<object> _saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand<object>(x => SaveEventType());
                }

                return _saveCommand;
            }
        }

        private ICommand _selectItemCommand;
        public ICommand SelectItemCommand
        {
            get
            {
                if (_selectItemCommand == null)
                {
                    _selectItemCommand = new RelayCommand<EventTypes>(x => _selectedEventType = x);
                }

                return _selectItemCommand;
            }
        }

        #endregion

        private void AddEventType()
        {
            //TODO: Add Parent event type?
            //Add childern to Parent event type
            if (_selectedEventType != null && _selectedEventType.ParentId == -1)
            {
                var newEvent = new EventTypes
                {
                    ParentId = _selectedEventType.EventTypeId,
                    EventTypeName = "New Event"
                };

                _selectedEventType.Children.Add(newEvent);
                var selectedEventType = EventTypesUI.EventTypes.FirstOrDefault(x => x.EventTypeId == _selectedEventType.EventTypeId);
                selectedEventType = _selectedEventType;
            }
        }

        private void RemoveEventType()
        {
            //TODO: Can I delete Parent event type with all childern?
            //TODO: Copy or Move event type childern? ==> MOVE
        }
        private void SaveEventType()
        {

        }

        private void RefreshEventTypeTree(EventItems events)
        {
            foreach (var item in events.EventTypes.ToList())
            {
                if (item.ParentId == -1)
                {
                    item.Children = new ObservableCollection<EventTypes>();
                    var eventsChilds = events.EventTypes.Where(child => child.ParentId == item.EventTypeId).ToList();

                    foreach (var child in eventsChilds)
                    {
                        item.Children.Add(child);
                    }

                    EventTypesUI.EventTypes.Add(item);
                }
            }

        }

    }
}