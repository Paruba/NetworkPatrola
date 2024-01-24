namespace NetworkMonitor
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }
    }
}