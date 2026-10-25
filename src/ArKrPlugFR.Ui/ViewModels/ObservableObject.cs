using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArKrPlugFR.Ui.ViewModels
{
    /// <summary>
    /// Base class for the row objects a tool window binds to.
    /// <para>
    /// The windows are data-bound, so a row that changes — a counter, a tick, a
    /// status — has to say so; this is the one place that is arranged.
    /// </para>
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Raises <see cref="PropertyChanged"/> for the calling member.</summary>
        /// <param name="propertyName">Name of the changed property; filled in by the compiler.</param>
        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>Assigns a field and raises the change notification when the value differs.</summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="field">Field behind the property.</param>
        /// <param name="value">New value.</param>
        /// <param name="propertyName">Name of the changed property; filled in by the compiler.</param>
        /// <returns><c>true</c> when the value changed.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            RaisePropertyChanged(propertyName);

            return true;
        }
    }
}
