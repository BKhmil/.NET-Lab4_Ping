using System.Diagnostics;
using System.IO.MemoryMappedFiles;

namespace Lab4_Ping
{
    public partial class Form1 : Form
    {
        // об'єкт MMF
        private MemoryMappedFile mmf;

        // м'ютекс для контролю доступу до пам'яті
        private Mutex mutex;

        public Form1()
        {
            InitializeComponent();

            // cтворення або відкриття спільної пам'яті
            mmf = MemoryMappedFile.CreateOrOpen("sharedColor", 100);

            // створення м'ютексу
            mutex = new Mutex(false, "colorMutex", out _);
        }

        // метод для обробки натискання кнопки для зміни кольору
        // створюю об'єкт Random кожен раз новий, для запобігання сайд-ефектам
        // на основі рандомного числа створюється колір, який встановлюється на Background
        // далі захоплюю м'ютекс -> записую до MMF створений колір у форматі ARGB -> звільняю м'ютекс
        private void button_change_Click(object sender, EventArgs e)
        {
            try
            {
                // Генеруємо випадковий колір
                Random rnd = new Random();
                Color color = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                this.BackColor = color;

                mutex.WaitOne();

                using (var stream = mmf.CreateViewStream())
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(color.ToArgb());
                }

                mutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error writing to shared memory: " + ex.Message);
            }
        }
    }
}
