using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MessageApp
{
    public partial class Form1 : Form
    {

        public string openkey;
        public string closekey;
        public string myToken = "";


        public Form1()
        {
            InitializeComponent();
        }
        private static string GetResponseString(string BaseUri, string json)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseUri);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (openkey != null)
            {
                string _myToken = Registration(textBox1.Text, textBox2.Text, textBox3.Text, openkey, closekey, richTextBox1);
                myToken = _myToken;
                richTextBox1.Text += _myToken + "\n";
            }
            else
            {
                richTextBox1.Text += "create keys" + "\n";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (openkey != null)
            {
                string _myToken = Authorization(textBox1.Text, textBox2.Text, textBox3.Text, openkey, closekey,  richTextBox1);
                myToken = _myToken;
                richTextBox1.Text += _myToken + "\n";
            }
            else
            {
                richTextBox1.Text += "create keys" + "\n";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SendMessage(textBox1.Text, textBox2.Text, textBox4.Text, richTextBox2.Text, myToken, richTextBox1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox3.Text += GetMessage(textBox1.Text, textBox2.Text, openkey, richTextBox1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RSAKeyGenerator(out openkey, out closekey);
            richTextBox1.Text += openkey + "\n";
            richTextBox1.Text += closekey + "\n";
        }


        public static string Registration(string uri, string name, string pass, string openkey, string closekey, RichTextBox richTextBox)
        {
            try
            {
                string BaseUri = uri + "registration";
                string json = "";
                string _openkey = GetResponseString(uri + "getkey", json).Split(':')[1].Trim('}').Trim('"');
                string _name = encript(name, _openkey);
                string _pass = encript(pass, _openkey);
                json = "{" + $"\"Name\":\"{_name}\", \"openkey\":\"{openkey}\", \"Password\":\"{_pass}\"" + "}";
                string answer = GetResponseString(BaseUri, json);
                string EnId = answer.Substring(9).TrimEnd('}').Trim('"');
                return decript(EnId, closekey, richTextBox);
                //return answer;
            }
            catch (Exception)
            {
                string _openkey = GetResponseString(uri + "getkey", "").Split(':')[1].Trim('}').Trim('"');
                richTextBox.Text += _openkey + "\n";
                return null;
            }
        }

        public static string Authorization(string uri, string name, string pass, string openkey, string closekey, RichTextBox richTextBox)
        {
            string BaseUri = uri + "authorization";
            string json = "";
            string _openkey = GetResponseString(uri + "getkey", json).Split(':')[1].Trim('}').Trim('"');
            string _name = encript(name, _openkey);
            string _pass = encript(pass, _openkey);
            json = "{" + $"\"Name\":\"{_name}\", \"openkey\":\"{openkey}\", \"Password\":\"{_pass}\"" + "}";
            string answer = GetResponseString(BaseUri, json);
            string EnId = answer.Substring(9).TrimEnd('}').Trim('"');
            //richTextBox.Text += answer + "\n";
            return decript(EnId, closekey, richTextBox);
        }

        public static string SendMessage(string uri, string myname, string Recipient, string messageText, string Token, RichTextBox richTextBox)
        {
            string BaseUri = uri + "sendmessage";
            string json = "";
            string RecipientKey = GetResponseString(uri + "getuserkey", json);

            string enMessage = messageText;//encript(messageText, RecipientKey);
            string hash = CreateMD5(Token + enMessage);
            json = "{" + $"\"Sender\":\"{myname}\", \"Recipient\":\"{Recipient}\", \"Hash\":\"{hash}\", \"messageText\":\"{messageText}\"" + "}";
            string answer = GetResponseString(BaseUri, json);
            richTextBox.Text += answer + "\n";
            return answer;
        }

        public static string GetMessage(string uri, string myname, string openkey, RichTextBox richTextBox)
        {
            string BaseUri = uri + "getmessages";
            string json = "";
            string _openkey = GetResponseString(uri + "getkey", json).Split(':')[1].Trim('}').Trim('"');
            string _name = encript(myname, _openkey);
            json = "{" + $"\"Name\":\"{_name}\", \"openkey\":\"{openkey}\"" + "}";
            string answer = GetResponseString(BaseUri, json);
            richTextBox.Text += answer + "\n";
            return answer;
        }

        public static void RSAKeyGenerator(out string publickey, out string privatekey)
        {
            RSACryptoServiceProvider RsaKey = new RSACryptoServiceProvider();
            publickey = RsaKey.ToXmlString(false); //получим открытый ключ
            privatekey = RsaKey.ToXmlString(true); //получим закрытый ключ
        }


        public static string decript(string ToDecrypt, string closekey, RichTextBox richTextBox)
        {
            try
            {
                RSACryptoServiceProvider RSA_ = new RSACryptoServiceProvider();
                //Create a UnicodeEncoder to convert between byte array and string.
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                //Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] Todecrypt = StringToBytes(ToDecrypt);
                byte[] Decrypted;

                RSA_.FromXmlString(closekey);
                Decrypted = RSADecrypt(Todecrypt, RSA_.ExportParameters(true), false);

                byte[] _Decrypted = new byte[Decrypted.Length / 2];//херня почему-то после каждого байта вставляет ноль
                for (int i = 0; i < Decrypted.Length; i += 2)
                {
                    _Decrypted[i / 2] = Decrypted[i];
                }
                return Encoding.UTF8.GetString(_Decrypted);
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Decryption failed.");
                return null;
            }
        }

        private static string BytesToString(byte[] decrypted)
        {
            string a = "";
            foreach (byte b in decrypted)
            {
                a += b + "|";
            }
            return a.TrimEnd('|');
        }

        public static string encript(string ToEncrypt, string OpenKey)
        {
            string Eid;
            try
            {
                RSACryptoServiceProvider RSA_ = new RSACryptoServiceProvider();

                RSA_.FromXmlString(OpenKey);

                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                byte[] IdToEncode = ByteConverter.GetBytes(ToEncrypt);
                byte[] EncodedId;

                EncodedId = RSAEncrypt(IdToEncode, RSA_.ExportParameters(false), false);
                Eid = BytesToString(EncodedId);
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Encryption failed.");
                Eid = null;
            }
            return Eid;
        }

        private static byte[] StringToBytes(string toEncrypt)
        {
            var a = toEncrypt.Split('|');
            byte[] b = new byte[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                b[i] = byte.Parse(a[i]);
            }
            return b;
        }

        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }

        public static string CreateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

    }
}
