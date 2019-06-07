using Eto.Forms.Plus;

namespace Demo
{
    public class MainFormViewModel : Screen
    {
        public string Hola { get; set; } = "Hola mundo desde una Screen";
        public void ChangeButton()
        {
            Hola = "Chaioooo";
        }
        public void Close()
        {
            RequestClose();
        }
    }
}
