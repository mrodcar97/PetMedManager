
namespace PetMedManager.Behaviors
{
    internal class DatePickerBehavior : Behavior<DatePicker>
    {
        protected override void OnAttachedTo(DatePicker bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.DateSelected += OnDateSelected;
        }

        protected override void OnDetachingFrom(DatePicker bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.DateSelected -= OnDateSelected;
        }

        void OnDateSelected(object sender, DateChangedEventArgs args)
        {
            DatePicker datePicker = (DatePicker)sender;
            DateTime selectedDate = args.NewDate;
            DateTime today = DateTime.Today;

            if (selectedDate > today)
            {
                var dateOnly = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day);
                datePicker.BackgroundColor = Color.FromHex("#bcd0eb");
                // Manejar la fecha válida según sea necesario
            }
            else
            {
               
                // Manejar la fecha inválida según sea necesario
            }
        }
    }    
}
