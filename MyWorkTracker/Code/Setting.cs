namespace MyWorkTracker.Code
{
    class Setting
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }

        private bool _userCanEdit;
        public bool UserCanEdit
        {
            get
            {
                return _userCanEdit;
            }
            set
            {
                if (value.Equals("Y"))
                    _userCanEdit = true;
                else if (value.Equals("N"))
                    _userCanEdit = false;
            }
        }

        public Setting(string name, string value, string defaultValue, string description, bool userCanEdit)
        {
            Name = name;
            Value = value;
            DefaultValue = defaultValue;
            Description = description;
            UserCanEdit = userCanEdit;
        }
    }
}
