using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyPALVGA
{
    public partial class Form1 : Form
    {
        char[] delimiterChars = { ' ', ',', '.', ':', '\t', '\r', '\n' };   // character used to separate values (can be changed by INPUT radio buttons)

        public Form1()
        {
            InitializeComponent();
        }

        // Takes the supplied text and runs a conversion on it
        // based on the options selected in the Output group box
        private string ConvertCode(string paletteCode)
        {
            int address = 0;
            string output = "";

            if (!rbTextFormat.Checked)
            {
                // prime the output
                output = "-- PALConverter-generated Memory Initialisation File (.mif)\r\n";
                output += "-- For use with Quartus II & Quartus Prime.\r\n";
                output += "-- Generated at: " + DateTime.Now + "\r\n";
                output += "\r\n";
                output += "WIDTH=16;\r\n";
                output += "DEPTH=256;\r\n";
                output += "\r\n";
                output += "ADDRESS_RADIX=HEX;\r\n";
                output += "DATA_RADIX=HEX;\r\n";
                output += "\r\n";
                output += "CONTENT BEGIN\r\n";
            } else
            {
                output = "palette=(\r\n";
            }

            // split the string into an array
            string[] values = paletteCode.Split(delimiterChars);

            if (values.Length < 2)
            {
                toolStripStatusLabel1.Text = "ERROR! Unable to split the code!";
                return null;
            }

            foreach (string value in values)
            {

                if (value.Length != 6)
                {
                    toolStripStatusLabel1.Text = "ERROR! Unrecognised value length!";
                    return null;
                }

                byte[] rgb = new byte[value.Length/2];

                for (int i = 0; i < rgb.Length; i++)
                {
                    rgb[i] = Convert.ToByte(value.Substring(i*2, 2), 16);
                }

                int[] rgb_out = new int[2];
                if (radioButton1.Checked)
                {
                    // Convert 3-byte RGB888 to 2-byte RGB565
                    rgb_out[1] = (rgb[0] & 0xF8) | (rgb[1] >> 5);
                    rgb_out[0] = ((rgb[1] & 0x1C) << 3) | (rgb[2] >> 3);
                } else
                {
                    // Convert 3-byte RGB888 to 2-byte RGB4444
                    rgb_out[1] = ((rgb[0] & 0xF0) >> 4);
                    rgb_out[0] = (rgb[1] & 0xF0) | ((rgb[2] & 0xF0) >> 4);
                }

                if (!rbTextFormat.Checked)
                {
                    output += "    " + String.Format("{0:X3}", address) + "  :   ";
                    output += String.Format("{0:X2}", rgb_out[1]) + String.Format("{0:X2}", rgb_out[0]) + ";\r\n";
                } else
                {
                    output += String.Format("{0:X2}", rgb_out[1]) + String.Format("{0:X2}", rgb_out[0]) + ", ";
                    if ((address + 1) % 8 == 0)
                    {
                        output += "\r\n";
                    }
                }

                address++;

                progressBar.Value = (address/values.Length) * 100;
            }

            if (!rbTextFormat.Checked)
            {
                if (address < 255)
                {
                    output += "    [" + String.Format("{0:X3}", address) + "..0FF]  :   0000;\r\n";
                }
                output += "END;\r\n";
            } else
            {
                // remove last space and comma and any CRLF if present
                output = output.Substring(0, output.LastIndexOf(','));

                // close the brackets
                output += ");";
            }

            // SUCCESS!
            if (radioButton1.Checked)
            {
                toolStripStatusLabel1.Text = "SUCCESS! Converted to RGB565.";
            } else
            {
                toolStripStatusLabel1.Text = "SUCCESS! Converted to RGB4444.";
            }

            progressBar.Value = 0;

            return output;

        }

        // Checks for text in the textbox before running a conversion on it
        private void StartConversion()
        {
            string filename = textBox2.Text;

            // get the pasted palette data
            if (textBox.Text == "")
            {
                // textBox is empty
                toolStripStatusLabel1.Text = "Nothing to convert!";
                return;
            } else
            {
                // check for a valid filename
                if (textBox2.Text == "")
                {
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.Filter = "Memory Initialisation File|*.mif|Text File|*.txt|Any file|*.*";
                    saveFileDialog1.Title = "Save Output";
                    saveFileDialog1.ShowDialog();
                    filename = saveFileDialog1.FileName;
                    if (filename == "")
                    {
                        // go for conversion into the text box
                        toolStripStatusLabel1.Text = "Converting, please wait...";
                        textBox.Text = ConvertCode(textBox.Text);
                        return;
                    }
                }

                if (textBox2.Text != "")
                {
                    if (!File.Exists(textBox2.Text))
                    {
                        using (StreamWriter sw = File.CreateText(textBox2.Text))
                        {
                            toolStripStatusLabel1.Text = "Converting, please wait...";
                            string output = ConvertCode(textBox.Text);
                            sw.Write(output);
                            sw.Close();
                        }
                    }
                } else
                {
                    toolStripStatusLabel1.Text = "Converting, please wait...";
                    textBox.Text = ConvertCode(textBox.Text);
                }
            }

        }

        private void ExitProgram()
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExitProgram();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitProgram();
        }

        private void convert_Click(object sender, EventArgs e)
        {
            StartConversion();
        }

        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartConversion();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormAbout().Show();
        }
    }
}