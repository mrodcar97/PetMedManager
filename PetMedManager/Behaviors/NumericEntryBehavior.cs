
namespace PetMedManager.Behaviors
{
    public class NumericEntryBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += OnEntryTextChanged;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged -= OnEntryTextChanged;
        }

        void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                bool isValid = int.TryParse(args.NewTextValue, out _);
                ((Entry)sender).TextColor = isValid ? default : new Color(1, 0, 0);
                if (!isValid)
                {
                    ((Entry)sender).Text = args.OldTextValue;
                }
            }
        }
    }
}

