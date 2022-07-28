using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Common
{

    public class CheckBoxModel
    {
        public CheckBoxModel()
        {
            Data = new List<CheckBox>();
        }
        public int KeyId { get; set; }
        public List<CheckBox> Data { get; set; }
    }


    public class CheckBox
    {
        public string Name { get; set; }

        public int Value { get; set; }

        public string Discription { get; set; }

        public bool IsChecked { get; set; }
    }
}
