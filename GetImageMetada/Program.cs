using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace ImageMetadataCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            string imagePath;

            if (args.Length > 0)
            {
                imagePath = args[0];
            }
            else
            {
                Console.WriteLine("Ingrese la ruta de la carpeta que contiene la imagen:");
                imagePath = Console.ReadLine();
            }

            if (!File.Exists(imagePath))
            {
                Console.WriteLine("La ruta proporcionada no es válida o la imagen no existe.");
                return;
            }

            try
            {
                ShowImageMetadata(imagePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer los metadatos de la imagen: {ex.Message}");
            }
        }

        static void ShowImageMetadata(string imagePath)
        {
            string outputFilePath = Path.ChangeExtension(imagePath, ".csv");
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                using (Image image = Image.FromFile(imagePath))
                {
                    PropertyItem[] propItems = image.PropertyItems;
                    writer.WriteLine("Datos de la imagen:");
                    writer.WriteLine($"Name,{Path.GetFileName(imagePath)}");
                    writer.WriteLine($"Creation Date,{File.GetCreationTime(imagePath)}");
                    writer.WriteLine($"Size,{image.Width}x{image.Height}");
                    writer.WriteLine("Metadata:");

                    writer.WriteLine("ID,Nombre del Metadato,Valor");
                    foreach (PropertyItem propItem in propItems)
                    {
                        string propName = GetPropertyName(propItem.Id);
                        if(propName != "Desconocido")
                        {
                            string propValue = GetPropertyValue(propItem);
                            writer.WriteLine($"{propItem.Id},{propName},{propValue}");
                        }
                    }
                }
            }
            Console.WriteLine($"Metadatos exportados a {outputFilePath}");
        }

        static string GetPropertyName(int id)
        {
            // Aquí puedes agregar más nombres de propiedades si es necesario
            switch (id)
            {
                case 0x010F: return "Equip";
                case 0x0110: return "Software";
                case 0x0132: return "ExpositionTime";
                case 0x013B: return "FocalLength";
                case 0x013E: return "FNumber";
                case 0x0112: return "Orientation";
                case 0x9003: return "DateTimeOriginal";
                case 0x9004: return "DateTimeDigitized";
                default: return "Desconocido";
            }
        }

        static string GetPropertyValue(PropertyItem propItem)
        {
            if (propItem.Id == 0x0112)
            {
                return GetOrientation(propItem);
            }
            else
            {
                // Convertir la cadena hexadecimal a un array de bytes
                byte[] byteArray = new byte[propItem.Len];
                for (int i = 0; i < propItem.Len; i++)
                {
                    byteArray[i] = Convert.ToByte(propItem.Value[i]);
                }

                // Convertir el array de bytes a un string usando UTF8
                string value = Encoding.UTF8.GetString(byteArray);

                // Eliminar caracteres nulos al final del string
                value = value.TrimEnd('\0');

                return value;
            }
        }

        static string GetOrientation(PropertyItem propItem)
        {
            // Convertir el valor del metadato a un entero
            int orientationValue = BitConverter.ToInt16(propItem.Value, 0);
            // Devolver el nombre de la orientación correspondiente
            switch (orientationValue)
            {
                case 1: return "Normal";
                case 2: return "Mirror vertical";
                case 3: return "Rotado  180 grados";
                case 4: return "Mirror horizontal";
                case 5: return "Rotado  90 grados y mirror vertical";
                case 6: return "Rotado  90 grados";
                case 7: return "Rotado  270 grados y mirror horizontal";
                case 8: return "Rotado  270 grados";
                default: return "Desconocido";
            }
        }

    }
}