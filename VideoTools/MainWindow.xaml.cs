using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using NReco.VideoInfo;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;

namespace VideoTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Convertir_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(RutaArchivo.Text))
            {
                string ruta = RutaArchivo.Text;
                Thread thread = new Thread(() => convertir(rutas));
                Estado.Content = "Loading";
                thread.Start();
            }
        }

        FFMpegConverter conv = new FFMpegConverter();
        string[] rutas;

        public void convertir(string[] rutas)
        {
            int cantidad = rutas.Count(),contador=0;
            foreach (var archivo in rutas)
            {

                string resultado = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(archivo), System.IO.Path.GetFileNameWithoutExtension(archivo) + ".mp4");
                if (File.Exists(resultado))
                    File.Delete(resultado);
                var inputFile = new FileStream(archivo, FileMode.Open);
                var outFile = new FileStream(resultado, FileMode.Create);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                conv.ConvertProgress += (a, b) =>
                {
                    double porcentaje = b.Processed.TotalMilliseconds * 100 / b.TotalDuration.TotalMilliseconds;
                    var tiempoTomado = stopwatch.Elapsed.TotalSeconds;
                    double restante = ((b.TotalDuration.TotalSeconds * tiempoTomado) / b.Processed.TotalSeconds) - tiempoTomado;
                    string medida = "seg.";
                    if (restante > 60)
                    {
                        restante = restante / 60;
                        medida = "min";
                    }
                    try
                    {
                        this.Dispatcher.Invoke((Action)(() => { Estado.Content = String.Format("{3}/{4} {0:0.00}% - time remaining: {1:0.00} {2}", porcentaje, restante, medida,contador,cantidad); }));
                    }
                    catch { }
                };

                conv.ConvertMedia(archivo, Format.matroska, outFile, Format.mp4, new ConvertSettings
                {
                    AudioCodec = "mp3",//mp3
                    VideoCodec = "copy",
                    CustomOutputArgs = "-map 0:0 -map 0:1?"
                });

                contador++;
            }

            this.Dispatcher.Invoke((Action)(() => { Estado.Content =cantidad+"/"+cantidad+ " Done!"; }));
        }

        private void Seleccionar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video|*.mkv";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                RutaArchivo.Text = String.Join(",", openFileDialog.FileNames.Select(r => System.IO.Path.GetFileName(r)).ToArray());
                rutas = openFileDialog.FileNames;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            conv.Abort();
            conv.Stop();
        }
    }
}
