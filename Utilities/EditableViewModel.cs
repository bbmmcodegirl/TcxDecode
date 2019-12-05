using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class EditableViewModel : ViewModel
    {
        private bool isDirty = false;

        public bool IsDirty { get => isDirty; }

        public new void Notify([CallerMemberName] string property = "")
        {
            if (string.IsNullOrWhiteSpace(property))
            {
                return;
            }
            isDirty = true;
            base.Notify(property);
            base.Notify(nameof(IsDirty));
        }

        public void Saved()
        {
            isDirty = false;
            base.Notify(nameof(IsDirty));
        }
    }
}
