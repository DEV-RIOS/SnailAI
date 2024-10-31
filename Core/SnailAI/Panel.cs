using Eto.Drawing;
using Eto.Forms;
using Grasshopper.Kernel.Types.Transforms;
using Rhino.PlugIns;
using Rhino.UI;
using Rhino.UI.Controls;
using System;

namespace SnailAI
{
    public class SnailPropertiesUiPanel : Panel
    {
        public Section _section;

        public SnailPropertiesUiPanel()
        {
            InitializeLayout();
        }

        public void InitializeLayout()
        {
            // Create holder for sections. The holder can expand/collapse sections and
            // displays a title for each section
            var holder = new EtoCollapsibleSectionHolder();

            // Create two sections
            _section = new Section(new LocalizeStringPair("Settings", "Settings"));

            // Populate the holder with sections
            holder.Add(_section);

            // Create a table layout that contains the holder and add it to the UI
            // Content
            var tableLayout = new TableLayout
            {
                Rows =
                {
                    holder
                }
            };

            Content = tableLayout;
        }
    }

    public class Section : EtoCollapsibleSection
    {
        public enum Season
        {
            Summer,
            Spring,
            Winter,
            Autumn
        }

        private Label _seasonLb;

        public EnumDropDown<Season> _seasonValue;

        public enum TimeOfDay
        {
            Morning,
            Midday,
            Afternoon,
            Evening,
            Night
        }

        private Label _timeOfDayLb;

        public EnumDropDown<TimeOfDay> _timeOfDayValue;


        public Section(LocalizeStringPair caption)
        {
            Caption = caption;
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            _seasonLb = new Label
            {
                Text = "Season"
            };

            _seasonValue = new EnumDropDown<Season>
            {
                Enabled = true,
                SelectedValue = Season.Summer,
                ToolTip = "Pick a season"
            };


            _timeOfDayLb = new Label
            {
                Text = "Time of Day"
            };

            _timeOfDayValue = new EnumDropDown<TimeOfDay>
            {
                Enabled = true,
                SelectedValue = TimeOfDay.Midday,
                ToolTip = "Pick a time of day"
            };

            _seasonValue.SelectedValueChanged += OnValueChange;
            _timeOfDayValue.SelectedValueChanged += OnValueChange;

            Content = new TableLayout(
            true, new TableRow(
                new TableCell(_seasonLb, true),
                new TableCell(_seasonValue)),
            new TableRow(
                new TableCell(_timeOfDayLb, true),
                new TableCell(_timeOfDayValue)))
            {
                Padding = 0
            };
        }

        public override LocalizeStringPair Caption { get; }

        public override int SectionHeight => Content.Height;

        private void OnValueChange(object sender, EventArgs e)
        {
            SnailAIPlugin.season = _seasonValue.SelectedValue.ToString();
            SnailAIPlugin.time = _timeOfDayValue.SelectedValue.ToString();
        }
    }
}