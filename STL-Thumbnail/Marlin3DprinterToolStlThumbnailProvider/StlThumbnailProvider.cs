using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using LogicNP.EZShellExtensions;
using Microsoft.Win32;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Image = System.Drawing.Image;
using Size = System.Windows.Size;
using System.Diagnostics;

/*
The following steps are only necessary if adding a new shell extension to an existing 
ClassLibrary project via the "Add New Item.." dialog box.
These are automatically performed when create a brand new shell extensions project is
created using the "New Project" dialog box.

STEP 1 
If the project's AssemblyInfo.cs file has the AssemblyVersion 
atrribute with a variable version as follows :
[assembly: AssemblyVersion("1.0.*")]
...change it to a constant version as follows :
[assembly: AssemblyVersion("1.0.0.0")]

STEP 2
This assembly must be given a strong name so that it can be installed in the GAC.
To do so, generate a strong name key pair using the sn.exe tool that comes with the .Net SDK as follows :
sn.exe -k keypair.kp 
Then copy the generated keypair.kp file to the project directory and add the following attribute
to the project's AssemblyInfo.cs file
[assembly: AssemblyKeyFile("..\\..\\keypair.kp")]
*/

/*
USING POST-BUILD STEPS 
Use the following Post-Build Steps to speed up testing and developing of your shell extension : 

"{Path To RegisterExtensionDotNetXX.exe}" -i "$(TargetPath)"
"{Path To RestartExplorer.exe}"

The first step registers the shell extension on the system and installs the dll in the GAC after 
every build. The second step restarts Windows Explorer so that your newly built dll gets loaded 
by Windows Explorer.

Example : 
"C:\Program Files\LogicNP Software\EZShellExtensions.Net\RegisterExtensionDotNetXX.exe" -i "$(TargetPath)"
"C:\Program Files\LogicNP Software\EZShellExtensions.Net\RestartExplorer.exe"

For Visual Studio 2002/2003 (.Net 1.0, 2.0), use RegisterExtensionDotNet10.exe
For Visual Studio 2005/2005 (.Net 2.0, 3.0, 3.5), use RegisterExtensionDotNet20.exe
For Visual Studio 2010 (.Net 4.0), use RegisterExtensionDotNet40.exe

See the topic "Using Post-Build Steps to ease testing of shell extensions" in the EZShellExtensions.Net 
help file for more information.
*/

namespace Marlin3DprinterToolStlThumbnailProvider
{
    // The GuidAttribute has been applied to the extension class
    // with an automatically generated unique GUID.
    // Every different extension should have such a unique GUID
    [Guid("847F91A7-50FE-4071-9BD3-034CA48D4251")]
    [ComVisible(true)]
    // The ThumbnailProviderAttribute attribute has been applied below to indicate 
    // the file types that your extension targets. 
    [ThumbnailProviderAttribute(".stl", true)]
    public class StlThumbnailProvider : ThumbnailProvider
    {

        // Called when the thumbnail is to be retrieved for the file 
        // specified by the TargetFile property.
        protected override Bitmap OnGetThumbnail(GetThumbnailEventArgs e)
        {
            return GetThumbnailImage(e.Width);
        }

        [STAThread]
        protected Bitmap GetThumbnailImage(int width) // Implemented abstract function in the base class
        {
            bool everythingisOk = true;


            

            
            MemoryStream memStream = new MemoryStream();
            



            Thread thread = new Thread(() =>
            {


                //string tracefile = @"C:\temp\stlviewer.stl_log";
                string tracefile = TargetFile.Replace(".stl", ".stl_log");

                
                Logg("_______________________________________________________________________________");
                FileInfo fileInfo = new FileInfo(TargetFile);
                Logg($"Name: {fileInfo.Name}");
                Logg($"Dir.: {fileInfo.Directory}");

                Logg($"Size: {fileInfo.Length}");
                Logg("1. Tread started");


                object convertFromString = null;
                Color color = new Color();
                ModelVisual3D root = new ModelVisual3D();
                Model3DGroup model = new Model3DGroup();
                PerspectiveCamera camera = new PerspectiveCamera();
                BitmapExporter exporter = new BitmapExporter();
                

                if (everythingisOk)
                {
                    try
                    {
                        string colorString = (string) Registry.GetValue(@"HKEY_CURRENT_USER\Software\3D model file", "Color","Yellow");

                        convertFromString = ColorConverter.ConvertFromString(colorString);
                        Logg($"2. Color: {convertFromString}");
                        if (convertFromString != null) color = (Color) convertFromString;
                    }
                    catch (Exception e)
                    {
                        Logg($"Something goes wrong in Color {e.Message}");
                        everythingisOk = false;
                    }
                }

                if (everythingisOk)
                {
                    if (convertFromString != null)
                    {
                        Viewport3D viewport = new Viewport3D();

                        if (everythingisOk)
                        {
                            try
                            {
                                Logg($"3. STLreader");
                                StLReader stlReader = new StLReader();
                                stlReader.DefaultMaterial = new DiffuseMaterial(new SolidColorBrush(color));

                                Logg($"4. Read the Targetfile {TargetFile}");
                                model = stlReader.Read(TargetFile);
                                Logg($"5. Model read. New Viewport");

                               
                            }
                            catch (Exception e)
                            {
                                Logg($"Something goes wrong in STLreader {e.Message}");
                                everythingisOk = false;
                            }
                        }



                        if (everythingisOk)
                        {
                            try
                            {
                                Logg($"6. Arrange with / height {width}/{width}");
                                viewport.Measure(new Size(width, width));
                                viewport.Arrange(new Rect(0, 0, width, width));

                                Logg($"7. Create root");

                                viewport.Children.Add(root);


                                Logg($"8. Create camera");

                                camera.Position = new Point3D(2, 16, 20);
                                camera.LookDirection = new Vector3D(-2, -16, -20);
                                camera.UpDirection = new Vector3D(0, 0, 1);
                                camera.FieldOfView = 45;
                                camera.NearPlaneDistance = 0.1;
                                camera.FarPlaneDistance = double.PositiveInfinity;
                                viewport.Camera = camera;

                            }
                            catch (Exception e)
                            {
                                Logg($"Something goes wrong in STLarrange {e.Message}");
                                everythingisOk = false;

                            }
                        }

                        if (everythingisOk)
                        {
                            try
                            {

                                Logg($"8.1. Create light");
                                root.Children.Add(new DefaultLights());
                                Logg($"8.2 Load model");
                                root.Content = model;
                                

                                camera.ZoomExtents(viewport);
                                Logg($"9. Create Background");
                                Brush background = new SolidColorBrush(Colors.Transparent);

                                Logg($"10. Create Exporter");

                                exporter.Background = background;
                                exporter.OversamplingMultiplier = 1;
                            }
                            catch (Exception e)
                            {

                                Logg($"Something goes wrong in Background/Light {e.Message}");
                                everythingisOk = false;
                            }
                        }

                        if (everythingisOk)
                        {
                            try
                            {
                                Logg($"11. Export to memorystream");
                                exporter.Export(viewport, memStream);
                                //Logg($"11. Export to {tempBitmap}");
                                //FileStream fs = new FileStream(tempBitmap, FileMode.Create, FileAccess.ReadWrite);
                                //exporter.Export(viewport, fs);
                                //fs.Flush();
                                //fs.Close();

                            }
                            catch (Exception e)
                            {
                                Logg($"Something goes wrong in Export to memory Stream {e.Message}");
                                everythingisOk = false;
                            }
                        }


                    }
                }

            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            

            

            if (everythingisOk)
            {
                try
                {
                    Logg($"12. Create Bitmap from memorystream");
                    Bitmap thumbnailBitmap = (Bitmap) Image.FromStream(memStream);
                    
                    return thumbnailBitmap;

                }
                catch (Exception e)
                {
                    Logg($"Something goes wrong in Bitmap from messageStream {e.Message}");
                    Logg("_______________________________________________________________________________");

                    return null;

                }
            }
            Logg("_______________________________________________________________________________");

            return null;
            


        }

        [ConditionalAttribute("DEBUG")]
        private void Logg(string text)
        {
            using (StreamWriter sw = File.AppendText(@"C:\temp\stl.log"))
            {
                sw.WriteLine(text);

            }
        }


        // Your assembly should have one static method marked with the 
        // ComRegisterFunction attribute. The function should return void and take 
        // one parameter whose type is System.Type.
        // 
        // This method is used to register the extension on the system by calling the
        // RegisterExtension method.
        //
        [ComRegisterFunction]
        public static void Register(Type t)
        {

            RegisterExtension(typeof(StlThumbnailProvider));
        }

        // Your assembly should have one static method marked with the 
        // ComUnregisterFunction attribute. The function should return void and take 
        // one parameter whose type is System.Type.
        // 
        // This method is used to register the extension on the system by calling the
        // UnRegisterExtension method.
        //
        [ComUnregisterFunction]
        public static void UnRegister(Type t)
        {
            UnRegisterExtension(typeof(StlThumbnailProvider));
        }


    }
}