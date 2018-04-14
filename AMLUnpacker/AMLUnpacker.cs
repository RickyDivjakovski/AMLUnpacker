using System;
using System.IO;

namespace AMLUnpacker
{
    public partial class AMLImageUnpacker
    {
        private void HexSplit(string inputFile, string outputFile, string startAddress, string endAddress)
        {
            FileStream hexReader = new FileStream(inputFile, FileMode.Open);
            FileStream hexWriter = new FileStream(outputFile, FileMode.Create);

            long StartAddress = Convert.ToInt64(startAddress.ToUpper(), 16);
            long EndAddress = Convert.ToInt64(endAddress.ToUpper(), 16);

            long bytecount = StartAddress;
            hexReader.Position = StartAddress;
            while (bytecount <= EndAddress)
            {
                hexWriter.WriteByte((byte)hexReader.ReadByte());
                bytecount++;
            }

            hexReader.Dispose();
            hexWriter.Dispose();
        }

        static string HexToString(string HexString)
        {
            string returnString = "";
            HexString = HexString.Replace(" ", "");
            for (int i = 0; i < HexString.Length / 2; i++)
            {
                string hexChar = HexString.Substring(i * 2, 2);
                int hexValue = Convert.ToInt32(hexChar, 16);
                returnString += Char.ConvertFromUtf32(hexValue);
            }
            return returnString;
        }

        public void Unpack(string inputFile, string outputFolder)
        {
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);
            HexSplit(inputFile, outputFolder + "\\head.BIN", "00000000", "000028C0");

            string LineContent = "";
            string PreviousLineContent = "";
            string FileName = "";
            string FileSize = "";
            string FileExtension = "";
            string StartAddress = "";
            string EndAddress = "";
            string TotalFile = "";
            int FileCount = 0;

            int CurrentByte = 0;
            byte currentblock;
            int ByteCount = 0;

            bool ddr = false;
            bool _aml_dtb = false;
            bool aml_sdc_burn = false;
            bool boot = false;
            bool bootloader = false;
            bool logo = false;
            bool platform = false;
            bool recovery = false;
            bool system = false;

            FileStream hexReader = new FileStream(outputFolder + "\\head.BIN", FileMode.Open);

            while (CurrentByte <= hexReader.Length)
            {
                currentblock = (byte)hexReader.ReadByte();
                if (ByteCount < 16)
                {
                    LineContent = LineContent + currentblock.ToString("X2") + " ";
                    ByteCount++;
                    if (LineContent.StartsWith("44 44 52") | LineContent.StartsWith("5F 61 6D 6C 5F 64 74 62") | LineContent.StartsWith("61 6D 6C 5F 73 64 63 5F 62 75 72 6E") | LineContent.StartsWith("62 6F 6F 74") | LineContent.StartsWith("62 6F 6F 74 6C 6F 61 64 65 72") | LineContent.StartsWith("6C 6F 67 6F") | LineContent.StartsWith("70 6C 61 74 66 6F 72 6D") | LineContent.StartsWith("72 65 63 6F 76 65 72 79") | LineContent.StartsWith("73 79 73 74 65 6D"))
                    {
                        if (LineContent.StartsWith("44 44 52") && !ddr) { FileName = HexToString("44 44 52"); ddr = true; }
                        else if (LineContent.StartsWith("5F 61 6D 6C 5F 64 74 62") && !_aml_dtb) { FileName = HexToString("5F 61 6D 6C 5F 64 74 62"); _aml_dtb = true; }
                        else if (LineContent.StartsWith("61 6D 6C 5F 73 64 63 5F 62 75 72 6E") && !aml_sdc_burn) { FileName = HexToString("61 6D 6C 5F 73 64 63 5F 62 75 72 6E"); aml_sdc_burn = true; }
                        else if (LineContent.StartsWith("62 6F 6F 74") && !boot) { FileName = HexToString("62 6F 6F 74"); boot = true; }
                        else if (LineContent.StartsWith("62 6F 6F 74 6C 6F 61 64 65 72") && !bootloader) { FileName = HexToString("62 6F 6F 74 6C 6F 61 64 65 72"); bootloader = true; }
                        else if (LineContent.StartsWith("6C 6F 67 6F") && !logo) { FileName = HexToString("6C 6F 67 6F"); logo = true; }
                        else if (LineContent.StartsWith("70 6C 61 74 66 6F 72 6D") && !platform) { FileName = HexToString("70 6C 61 74 66 6F 72 6D"); platform = true; }
                        else if (LineContent.StartsWith("72 65 63 6F 76 65 72 79") && !recovery) { FileName = HexToString("72 65 63 6F 76 65 72 79"); recovery = true; }
                        else if (LineContent.StartsWith("73 79 73 74 65 6D") && !system) { FileName = HexToString("73 79 73 74 65 6D"); system = true; }
                        if (FileExtension != "" && StartAddress != "" && FileSize != "" && EndAddress != "" && FileName != "")
                        {
                            HexSplit(inputFile, outputFolder + "\\" + FileName + FileExtension, StartAddress, EndAddress);
                            TotalFile = TotalFile + FileName + FileExtension + "\nStart address: " + StartAddress + "\nEnd address: " + EndAddress + "\nFile size: " + (Convert.ToInt64(FileSize.ToUpper(), 16).ToString()) + "\n\n";
                            FileExtension = "";
                            StartAddress = "";
                            FileSize = "";
                            EndAddress = "";
                            FileName = "";
                        }
                    }
                }
                else
                {
                    if (LineContent.StartsWith("55 53 42") | LineContent.StartsWith("69 6E 69") | LineContent.StartsWith("63 6F 6E 66") | LineContent.StartsWith("50 41 52 54 49 54 49 4F 4E"))
                    {
                        FileCount++;

                        if (LineContent.StartsWith("55 53 42")) FileExtension = ".USB";
                        else if (LineContent.StartsWith("50 41 52 54 49 54 49 4F 4E")) FileExtension = ".PARTITION";
                        else if (LineContent.StartsWith("69 6E 69")) FileExtension = ".ini";
                        else if (LineContent.StartsWith("63 6F 6E 66")) FileExtension = ".conf";

                        if (PreviousLineContent.Split()[3] == "0") StartAddress = PreviousLineContent.Split()[2] + PreviousLineContent.Split()[1] + PreviousLineContent.Split()[0];
                        else StartAddress = PreviousLineContent.Split()[3] + PreviousLineContent.Split()[2] + PreviousLineContent.Split()[1] + PreviousLineContent.Split()[0];

                        if (PreviousLineContent.Split()[11] == "0") FileSize = PreviousLineContent.Split()[10] + PreviousLineContent.Split()[9] + PreviousLineContent.Split()[8];
                        else FileSize = PreviousLineContent.Split()[11] + PreviousLineContent.Split()[10] + PreviousLineContent.Split()[9] + PreviousLineContent.Split()[8];

                        EndAddress = ((Convert.ToInt64(StartAddress.ToUpper(), 16) + (Convert.ToInt64(FileSize.ToUpper(), 16))) - 1).ToString("X");
                    }
                    PreviousLineContent = LineContent;
                    LineContent = currentblock.ToString("X2") + " ";

                    ByteCount = 1;
                }
                CurrentByte++;
            }

            hexReader.Dispose();
            File.Delete(outputFolder + "\\head.BIN");

            System.IO.File.WriteAllText(outputFolder + "\\partition_structure.txt", TotalFile);
        }
    }
}
