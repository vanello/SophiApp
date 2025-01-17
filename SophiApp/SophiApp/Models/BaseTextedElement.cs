﻿using SophiApp.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SophiApp.Models
{
    internal class BaseTextedElement : INotifyPropertyChanged
    {
        private string description;
        private string header;
        private bool isChecked;
        private bool isEnabled;

        internal Action<uint, Exception> ErrorOccurred;
        internal Action<uint, UIElementState> StateChanged;
        public const string DescriptionPropertyName = "Description";
        public const string HeaderPropertyName = "Header";
        public const string IsCheckedPropertyName = "IsChecked";
        public const string IsEnabledPropertyName = "IsEnabled";

        public Func<bool> CurrentStateAction;
        public Action<bool> SystemStateAction;

        public BaseTextedElement()
        {
        }

        public BaseTextedElement(JsonDTO json)
        {
            Id = json.Id;
            Descriptions = json.Description;
            Headers = json.Header;
            Tag = json.Tag;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal Dictionary<UILanguage, string> Descriptions { get; set; }
        internal Dictionary<UILanguage, string> Headers { get; set; }

        public uint ContainerId { get; set; }

        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(DescriptionPropertyName);
            }
        }

        public string Header
        {
            get => header;
            set
            {
                header = value;
                OnPropertyChanged(HeaderPropertyName);
            }
        }

        public uint Id { get; set; }

        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                OnPropertyChanged(IsCheckedPropertyName);
            }
        }

        public bool IsClicked { get; set; }
        public bool IsContainer { get; set; }

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged(IsEnabledPropertyName);
            }
        }

        public UIElementState State
        {
            get => GetState();
            set
            {
                SetState(value);
                StateChanged?.Invoke(Id, value);
            }
        }

        public string Tag { get; set; }

        private UIElementState GetState()
        {
            if (IsEnabled & IsChecked & IsClicked == false)
                return UIElementState.CHECKED;

            if (IsEnabled & IsChecked == false & IsClicked == false)
                return UIElementState.UNCHECKED;

            if (IsEnabled & IsChecked == false & IsClicked)
                return UIElementState.SETTODEFAULT;

            if (IsEnabled & IsChecked & IsChecked)
                return UIElementState.SETTOACTIVE;

            return UIElementState.DISABLED;
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SetState(UIElementState value)
        {
            switch (value)
            {
                case UIElementState.DISABLED:
                    IsEnabled = IsChecked = IsClicked = false;
                    break;

                case UIElementState.CHECKED:
                    IsEnabled = IsChecked = true;
                    IsClicked = false;
                    break;

                case UIElementState.UNCHECKED:
                    IsEnabled = true;
                    IsChecked = IsClicked = false;
                    break;

                case UIElementState.SETTODEFAULT:
                    IsEnabled = IsClicked = true;
                    IsChecked = false;
                    break;

                case UIElementState.SETTOACTIVE:
                    IsEnabled = IsChecked = IsClicked = true;
                    break;
            }
        }

        internal void ChangeState()
        {
            switch (State)
            {
                case UIElementState.CHECKED:
                    State = UIElementState.SETTODEFAULT;
                    break;

                case UIElementState.UNCHECKED:
                    State = UIElementState.SETTOACTIVE;
                    break;

                case UIElementState.SETTODEFAULT:
                    State = UIElementState.CHECKED;
                    break;

                case UIElementState.SETTOACTIVE:
                    State = UIElementState.UNCHECKED;
                    break;
            }
        }

        internal void GetCurrentState()
        {
            try
            {
                State = CurrentStateAction?.Invoke() == true ? UIElementState.CHECKED : UIElementState.UNCHECKED;
            }
            catch (Exception e)
            {
                ErrorOccurred?.Invoke(Id, e);
            }
        }

        internal virtual void SetLocalization(UILanguage language)
        {
            Header = Headers[language];
            Description = Descriptions[language];
        }

        internal void SetSystemState()
        {
            try
            {
                SystemStateAction(State == UIElementState.SETTOACTIVE);
            }
            catch (Exception e)
            {
                ErrorOccurred?.Invoke(Id, e);
            }
        }
    }
}