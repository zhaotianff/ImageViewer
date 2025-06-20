using FellowOakDicom;
using FellowOakDicom.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System;
using System.Linq;
using System.Management;
using FellowOakDicom.Imaging.NativeCodec;

namespace DicomRenderImageTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Application started.");
            Console.WriteLine($"Operating system: {GetOperatingSystem()}"); // <-- Windows 11 PRO
            if (IntPtr.Size == 8)
                Console.WriteLine("Platform: x64"); // <---- x64
            else
                Console.WriteLine("Platform: x86");

            //try
            //{
                DicomException.OnException += delegate (object sender, DicomExceptionEventArgs ea)
                {
                    Console.WriteLine($"Exception: {ea.Exception.Message}{Environment.NewLine}{ea.Exception.StackTrace}");
                };

                new DicomSetupBuilder().RegisterServices(s => s
                    .AddFellowOakDicom()
                    .AddTranscoderManager<NativeTranscoderManager>() //for second image (0002.DCM) needed
                    .AddImageManager<ImageSharpImageManager>()
                    .AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Trace))
                ).SkipValidation().Build();

                Console.WriteLine("Rendering of first image (0020.DCM) started.");
                DicomImage dicomImage = new DicomImage("0020.DCM");
                var image = dicomImage.RenderImage();
                image.AsSharpImage().SaveAsPng("0020.DCM-Export.png");
                Console.WriteLine("Rendering of first image (0020.DCM) completed.");

                Console.WriteLine("Rendering of second image (0002.DCM) started.");
                DicomImage dicomImage2 = new DicomImage("0002.DCM");
                var image2 = dicomImage2.RenderImage(); // ---------------------- Exception: DLL "Dicom.Native" not found !? -----------------
                image2.AsSharpImage().SaveAsPng("0002.DCM-Export.png");
                Console.WriteLine("Rendering of second image (0002.DCM) completed.");
            //}catch(Exception e)
            //{
            //    Console.WriteLine($"Exception: {e.Message}{Environment.NewLine}{e.StackTrace}");
            //}

            Console.WriteLine("Application completed.");
            Console.ReadKey();
        }

        private static string GetOperatingSystem()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                        select x.GetPropertyValue("Caption")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }
    }
}
