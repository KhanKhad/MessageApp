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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.IO;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using System.Net;

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
        private static string GetResponseStringPost(string BaseUri, string json)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseUri);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
        }
        private static string GetResponseStringGet(string BaseUri, string json)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseUri);
            return httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
        }


        private string Upload(string actionUrl, string paramString, Stream paramFileStream, byte[] paramFileBytes)
        {
            HttpContent stringContent = new StringContent(paramString);
            HttpContent fileStreamContent = new StreamContent(paramFileStream);
            HttpContent bytesContent = new ByteArrayContent(paramFileBytes);
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(new StringContent("fdfg"), "param1");
                formData.Add(new StringContent("fdg"), "username");
                //formData.Add(fileStreamContent, "file1.docx", "file1");
                formData.Add(bytesContent, "file2", "file2.docx");
                
                var response = client.PostAsync(actionUrl, formData);

                File.WriteAllBytes("D://download//we2.docx", response.Result.Content.ReadAsByteArrayAsync().Result);

                return ""; //response.Result.Content.ReadAsStringAsync().Result;
            }
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
            SendMessage(textBox1.Text, textBox2.Text, textBox4.Text, richTextBox2.Text, myToken,openkey, closekey, richTextBox1);
        }

        private void button4_Click(object sender, EventArgs e)
        {

            //GetMessage(textBox1.Text, textBox2.Text, myToken, openkey, closekey, richTextBox3);
            richTextBox1.Text += Upload(textBox1.Text + "filesupload", "", File.OpenRead("D://download//we.docx"), new StreamContent(File.OpenRead("D://download//lab3.docx")).ReadAsByteArrayAsync().Result);
        }
        public static string decrypt(string text, string privateKey)
        {
            byte[] cypherText = Convert.FromBase64String(text);
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                var bytesText = rsa.Decrypt(cypherText, false);
                return Encoding.UTF8.GetString(bytesText);
            }
        }
        public static string decrypt(byte[] cypherText, string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                var bytesText = rsa.Decrypt(cypherText, false);
                return Encoding.UTF8.GetString(bytesText);
            }
        }
        public static RSACryptoServiceProvider ImportPrivateKey(string pem)
        {
            PemReader pr = new PemReader(new StringReader(pem));
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
        }
        public static RSACryptoServiceProvider ImportPublicKey(string pem)
        {
            PemReader pr = new PemReader(new StringReader(pem));
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
        }
        public static string ExportPublicKey(RSACryptoServiceProvider csp)
        {
            StringWriter outputStream = new StringWriter();
            var parameters = csp.ExportParameters(false);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    innerWriter.Write((byte)0x30); // SEQUENCE
                    EncodeLength(innerWriter, 13);
                    innerWriter.Write((byte)0x06); // OBJECT IDENTIFIER
                    var rsaEncryptionOid = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
                    EncodeLength(innerWriter, rsaEncryptionOid.Length);
                    innerWriter.Write(rsaEncryptionOid);
                    innerWriter.Write((byte)0x05); // NULL
                    EncodeLength(innerWriter, 0);
                    innerWriter.Write((byte)0x03); // BIT STRING
                    using (var bitStringStream = new MemoryStream())
                    {
                        var bitStringWriter = new BinaryWriter(bitStringStream);
                        bitStringWriter.Write((byte)0x00); // # of unused bits
                        bitStringWriter.Write((byte)0x30); // SEQUENCE
                        using (var paramsStream = new MemoryStream())
                        {
                            var paramsWriter = new BinaryWriter(paramsStream);
                            EncodeIntegerBigEndian(paramsWriter, parameters.Modulus); // Modulus
                            EncodeIntegerBigEndian(paramsWriter, parameters.Exponent); // Exponent
                            var paramsLength = (int)paramsStream.Length;
                            EncodeLength(bitStringWriter, paramsLength);
                            bitStringWriter.Write(paramsStream.GetBuffer(), 0, paramsLength);
                        }
                        var bitStringLength = (int)bitStringStream.Length;
                        EncodeLength(innerWriter, bitStringLength);
                        innerWriter.Write(bitStringStream.GetBuffer(), 0, bitStringLength);
                    }
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                // WriteLine terminates with \r\n, we want only \n
                outputStream.Write("-----BEGIN PUBLIC KEY-----\n");
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.Write(base64, i, Math.Min(64, base64.Length - i));
                    outputStream.Write("\n");
                }
                outputStream.Write("-----END PUBLIC KEY-----");
            }

            return outputStream.ToString();
        }
        public static string ExportPrivateKey(RSACryptoServiceProvider csp)
        {
            StringWriter outputStream = new StringWriter();
            if (csp.PublicOnly) throw new ArgumentException("CSP does not contain a private key", "csp");
            var parameters = csp.ExportParameters(true);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
                    EncodeIntegerBigEndian(innerWriter, parameters.D);
                    EncodeIntegerBigEndian(innerWriter, parameters.P);
                    EncodeIntegerBigEndian(innerWriter, parameters.Q);
                    EncodeIntegerBigEndian(innerWriter, parameters.DP);
                    EncodeIntegerBigEndian(innerWriter, parameters.DQ);
                    EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                // WriteLine terminates with \r\n, we want only \n
                outputStream.Write("-----BEGIN RSA PRIVATE KEY-----\n");
                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.Write(base64, i, Math.Min(64, base64.Length - i));
                    outputStream.Write("\n");
                }
                outputStream.Write("-----END RSA PRIVATE KEY-----");
            }

            return outputStream.ToString();
        }
        private static void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }
        private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }
        
        private void button5_Click(object sender, EventArgs e)
        {
            RSAKeyGenerator(out openkey, out closekey);
            richTextBox1.Text += openkey + "\n";
            richTextBox1.Text += closekey + "\n";
        }
        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox4.Text += GetInformAboutSendedMessages(textBox1.Text, textBox2.Text, openkey, richTextBox1);
        }

        public static string GetConfurm(string uri, string name, string myToken, int opId, string openkeyserver, string openkey, string privateKey)
        {
            string json = "{" + $"\"operationId\":\"{encrypt(opId.ToString(), openkeyserver)}\", \"hashName\":\"{encrypt(CreateMD5(name), openkeyserver)}\", \"confurmStringClient\":\"{encrypt(myToken, openkeyserver)}\", \"openkey\":\"{openkey}\"" + "}";
            string answer = GetResponseStringPost(uri + "getconfurm", json);
            string serverTokenEncrypted = answer.Split(':')[1].Trim('}').Trim('"');
            string serverToken = decrypt(serverTokenEncrypted, privateKey);
            return serverToken.Split('|')[1];
        }
        public static string GetConfurm(string uri, string name, string myToken, int opId, string openkeyserver, string openkey, string privateKey, RichTextBox rich)
        {
            string json = "{" + $"\"operationId\":\"{encrypt(opId.ToString(), openkeyserver)}\", \"hashName\":\"{encrypt(CreateMD5(name), openkeyserver)}\", \"confurmStringClient\":\"{encrypt(myToken, openkeyserver)}\", \"openkey\":\"{openkey}\"" + "}";
            string answer = GetResponseStringPost(uri + "getconfurm", json);
            rich.Text += answer+"\n";
            string serverTokenEncrypted = answer.Split(':')[1].Trim('}').Trim('"');
            string serverToken = decrypt(serverTokenEncrypted, privateKey);
            rich.Text += serverToken + "\n";
            return serverToken.Split('|')[1];
        }
        public static string Registration(string uri, string name, string pass, string openkey, string closekey, RichTextBox richTextBox)
        {
            string _myToken = Guid.NewGuid().ToString();
            string json = "";
            string _openkey = GetResponseStringPost(uri + "getkeyxml", json).Split(':')[1].Trim('}').Trim('"');
            string _serverToken = GetConfurm(uri, name, _myToken, 0, _openkey, openkey, closekey);

            string BaseUri = uri + "registration";
            string _name = encrypt(_serverToken + "|" + name + "|" + _myToken, _openkey);
            string _pass = encrypt(_myToken + "|" + pass + "|" + _serverToken, _openkey);
            json = "{" + $"\"Name\":\"{_name}\", \"openkey\":\"{openkey}\", \"Password\":\"{_pass}\"" + "}";
            string answer = GetResponseStringPost(BaseUri, json);
            string EnId = answer.Substring(9).TrimEnd('}').Trim('"');
            return decrypt(EnId, closekey).Split('|')[1];
        }

        public static string Authorization(string uri, string name, string pass, string openkey, string closekey, RichTextBox richTextBox)
        {
            string _myToken = Guid.NewGuid().ToString();
            string json = "";
            string _openkey = GetResponseStringPost(uri + "getkeyxml", json).Split(':')[1].Trim('}').Trim('"');
            string _serverToken = GetConfurm(uri, name, _myToken, 1, _openkey, openkey, closekey);

            string BaseUri = uri + "authorization";
            string _name = encrypt(_serverToken + "|" + name + "|" + _myToken, _openkey);
            string _pass = encrypt(_myToken + "|" + pass + "|" + _serverToken, _openkey);
            json = "{" + $"\"Name\":\"{_name}\", \"openkey\":\"{openkey}\", \"Password\":\"{_pass}\"" + "}";
            string answer = GetResponseStringPost(BaseUri, json);
            string EnId = answer.Substring(9).TrimEnd('}').Trim('"');
            return decrypt(EnId, closekey).Split('|')[1];
        }

        public static string SendMessage(string uri, string myname, string Recipient, string messageText, string Token, string openkey, string closekey, RichTextBox richTextBox)
        {
            string _myToken = Guid.NewGuid().ToString();
            string json = "";
            string _openkey = GetResponseStringPost(uri + "getkeyxml", json).Split(':')[1].Trim('}').Trim('"');
            string _serverToken = GetConfurm(uri, myname, _myToken, 2, _openkey, openkey, closekey);


            string BaseUri = uri + "sendmessage";
            string RecipientKey = GetResponseStringGet(uri + "getuserkeyxml?recipient=" + Recipient, json).Split(':')[1].Trim('}').Trim('"');

            string enMessage = encrypt(myname + "|" + messageText, RecipientKey);
            string hash = CreateMD5(Token + enMessage + _serverToken);
            json = "{" + $"\"Sender\":\"{CreateMD5(myname)}\", \"Recipient\":\"{CreateMD5(Recipient)}\", \"Hash\":\"{hash}\", \"messageText\":\"{enMessage}\"" + "}";
            string answer = GetResponseStringPost(BaseUri, json);
            richTextBox.Text += answer + "\n";
            return answer;



            
        }
        public static string GetMessage(string uri, string myname, string Token, string openkey, string closekey,RichTextBox richTextBox)
        {
            string _myToken = Guid.NewGuid().ToString();
            string json = "";
            string _openkey = GetResponseStringPost(uri + "getkeyxml", json).Split(':')[1].Trim('}').Trim('"');
            string _serverToken = GetConfurm(uri, myname, _myToken, 3, _openkey, openkey, closekey);

            string BaseUri = uri + "getmessages";

            string _name = CreateMD5(myname);
            string _hash = CreateMD5(Token + _name + _serverToken);
            json = "{" + $"\"Name\":\"{_name}\", \"openkey\":\"{openkey}\", \"hashkey\":\"{_hash}\"" + "}";
            string answer = GetResponseStringPost(BaseUri, json).Substring(19).TrimEnd('}').TrimEnd('"');
            
            foreach (string line in answer.Trim('}').Trim('"').Split('#'))
            {
                string text = decrypt(line.Split('|')[3], closekey);
                string sendername = text.Split('|')[0];
                if (line.Split('|')[2].Equals(CreateMD5(sendername)))
                {
                    richTextBox.Text += line.Split('|')[0] + " : " + sendername + " : " + text.Split('|')[1] + "\n";
                }
            }

            return answer;
        }

        public static string GetInformAboutSendedMessages(string uri, string myname, string openkey, RichTextBox richTextBox)
        {
            string BaseUri = uri + "checkMessagesInfo";
            string json = "";
            string _openkey = GetResponseStringPost(uri + "getkeyxml", json).Split(':')[1].Trim('}').Trim('"');
            string _name = encrypt(myname, _openkey);
            json = "{" + $"\"Name\":\"{_name}\", \"openkey\":\"{openkey}\"" + "}";
            string answer = GetResponseStringPost(BaseUri, json);
            richTextBox.Text += answer + "\n";
            return answer;
        }

        public static void RSAKeyGenerator(out string publickey, out string privatekey)
        {
            RSACryptoServiceProvider RsaKey = new RSACryptoServiceProvider();
            publickey = RsaKey.ToXmlString(false); //получим открытый ключ
            privatekey = RsaKey.ToXmlString(true); //получим закрытый ключ
        }


       /* public static string decript(string ToDecrypt, string closekey)
        {
            //return ToDecrypt;
            try
            {
                RSACryptoServiceProvider RSA_ = new RSACryptoServiceProvider();
                //Create a UnicodeEncoder to convert between byte array and string.
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                //Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] Todecrypt = Convert.FromBase64String(ToDecrypt);//StringToBytes(ToDecrypt);//wrong operation token2
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
        }*/

        private static string BytesToString(byte[] decrypted)
        {
            string a = "";
            foreach (byte b in decrypted)
            {
                a += b + "|";
            }
            return a.TrimEnd('|');
        }

        public static string encrypt(string ToEncrypt, string OpenKey)
        {
            //return ToEncrypt;
            string Eid;
            try
            {
                RSACryptoServiceProvider RSA_ = new RSACryptoServiceProvider();

                RSA_.FromXmlString(OpenKey);

                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                byte[] IdToEncode = Encoding.UTF8.GetBytes(ToEncrypt);
                byte[] EncodedId;

                EncodedId = RSAEncrypt(IdToEncode, RSA_.ExportParameters(false), false);

                Eid = Convert.ToBase64String(EncodedId);

            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Encryption failed.");
                Eid = null;
            }
            return Eid;
        }
        public static string encrypt(byte[] IdToEncode, string OpenKey)
        {
            string Eid;
            try
            {
                RSACryptoServiceProvider RSA_ = new RSACryptoServiceProvider();

                RSA_.FromXmlString(OpenKey);

                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                byte[] EncodedId;

                EncodedId = RSAEncrypt(IdToEncode, RSA_.ExportParameters(false), false);

                Eid = Convert.ToBase64String(EncodedId);

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
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
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

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = "http://178.21.10.180/";
        }
    }
}
