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
            {
                string op = "-----BEGIN RSA PUBLIC KEY-----MIIBCgKCAQEAj/ggukI20t7asxRrBoJmhIQVVQ31FXIS8y1xW+DXzKxEYgr52dlLzQ9DN1N53vdqGO3hL+CdezNLfk3N+jt+2XFmNlnWwIBW4BztIxwPE4bXhCMyTYg+14ZyK8BA3Jbm6XWMVbnmOwfAf1KVCLgUVcCoEfg9ncOIOpwR1ZdOv9V3pAGipoPvWB3KJKuOlnREotbJgug7YklX0LMnWIEN6VrEbXSWw6HFEROR+Kfazd+Bz3yaJBEQFxj6PHTh9LKEwTVobyac0B+QVa+kVuA8u5XgoKijIR0SuQmpdLI4+K8skxRPWedwWTj4iUfDg3uiPUcVaDn3nU96WGSFjonzOQIDAQAB-----END RSA PUBLIC KEY-----";
                string pop = "-----BEGIN RSA PRIVATE KEY-----MIIEogIBAAKCAQEA08kDVqVGCWZdvapAxX6tKrgOXqq9WAxrYRX/4WSLuhrOlHf3KtzfrccGjgjYRwb9FnPLMd3TkDWUR62u9HEcK93cRpQHklSOxuQxXc+8EjP7JNb9oRYZGnNBIAKztEO11Eo6q/TJNalca+aPTXWY+DxZ+O6xn9t9sVPD4zzcpFkHkDDd2on7ub9EPwAwl4rfy2n4/foPuEbxQehGFCpb3eb8pLOxQUi1OKyFmrtQ4MFIYg0Xussx3PON38R7bWv+4oZgy0mz8a/UyPOAb2zxn7M8NMjfbVtCK9XWqHWj80KGU8La94OHYMTV5QYAxQsBrrc3lnUHWELZSCwnj3Bs3QIDAQABAoH/HZgE08VEhCMD9ln2+OfGSuBtUCpF1rZxM4iN/s4CW1CSMEQ2IruL+msPc8mCzGoKrccMHcjV6kU1UEMvDITwR+shXLtp9logU5NJr8aRpCgTtdf33twUvTS62VWHUp2X7dgUcgorWXcuIYitfaVxI216Dc7v1KDizJM5Y3vjh58FJwzbZXMjRURTRmk3z+LRXwdz9Nh08rBDHtOr9fhuFlyiYKVqNVa5faGFv/1sWtdhxY3exkbUf99z8VH+EPc3W1iG73uBSpUtNGKODMakOxN5faKNH4sl+MXVUjfpSn1+LUij9owXl1pnspIs/AQk/KqA9lBd+kuKyVTiE4uPAoGBAPQYFFroalVEwaYtKv4Zz5gT+Y2OB5ZjJ5bmmpVQnwZevA/b+EOZHoeJsDN87HGAhtRTNCcnyyHVNx7VxTPegnAmDFrOcjiYpTA5Jat3swtHD9rhoD/5I8eHg0B282MTHplWQ7S1YuD+C4IJ1GmER9vz8EAey4NLurbJHMLPPXcTAoGBAN4dgPTadZtya4dTJpGBN0KffLzcu8vOY8kFHbwKvY778L00q//GjYucLVgsW3UkbxTqjCHxQf9qeMojXcBmjRs6kZ/JYNpS4jYvZ3Yf+Jx1VSSrLYbWGY+tBO/zVabyvGe6dbjSgyRBnGdV/22LgX4vSNJxfDjFwotOMfYpdFpPAoGBAJ0apndAYbgR8eWH6sTzTebe7F7MjFuh5Ag+0j3KTyAldztp4+d2NO0dlLf+7pu7Eoy0JLwS464Z8kk5Y5yX2TJfetIzT9bXgHKCRZmQCD/48954G9ExBHNW6AZkyQ/6bVZZ7Gfx2vte539B3mIvjqSl4/sRFwGhi0I8PPOxuSjbAoGBAJJxL1nAQvQXL1AMgYxDfnSdhHdcrTCFRgL+LYmSJ0KDV0jX1mMPvLkEYl0U+cO1HsvSjEvArfvBbhwPzzsQIg6GwgZwlju7k8uX24XlNFurfFRty9lvhXDV6UBu1dT5i0B0jjMqEC6yV3VGHN9TsC/K6x9clUM1F7wS9RvQXxc/AoGAH6FIsmLNDJkwOntyy/XayRbyDAf6oktVvjRlRoUDhwaKzgCqeZ601gJT6naO8Jovh30wLbSHrflhM2oy8Wyi0dknGnPHm0QifmI+JSYMBVwS267j8OcDROWJ04zHbv20jgi5y8myV7jzbnU01MUyhoOiX1f3gU7OzkKKqcG4IPU=-----END RSA PRIVATE KEY-----";
                /*
                 MIIEogIBAAKCAQEA08kDVqVGCWZdvapAxX6tKrgOXqq9WAxrYRX/4WSLuhrOlHf3KtzfrccGjgjYRwb9FnPLMd3TkDWUR62u9HEcK93cRpQHklSOxuQxXc+8EjP7JNb9oRYZGnNBIAKztEO11Eo6q/TJNalca+aPTXWY+DxZ+O6xn9t9sVPD4zzcpFkHkDDd2on7ub9EPwAwl4rfy2n4/foPuEbxQehGFCpb3eb8pLOxQUi1OKyFmrtQ4MFIYg0Xussx3PON38R7bWv+4oZgy0mz8a/UyPOAb2zxn7M8NMjfbVtCK9XWqHWj80KGU8La94OHYMTV5QYAxQsBrrc3lnUHWELZSCwnj3Bs3QIDAQABAoH/HZgE08VEhCMD9ln2+OfGSuBtUCpF1rZxM4iN/s4CW1CSMEQ2IruL+msPc8mCzGoKrccMHcjV6kU1UEMvDITwR+shXLtp9logU5NJr8aRpCgTtdf33twUvTS62VWHUp2X7dgUcgorWXcuIYitfaVxI216Dc7v1KDizJM5Y3vjh58FJwzbZXMjRURTRmk3z+LRXwdz9Nh08rBDHtOr9fhuFlyiYKVqNVa5faGFv/1sWtdhxY3exkbUf99z8VH+EPc3W1iG73uBSpUtNGKODMakOxN5faKNH4sl+MXVUjfpSn1+LUij9owXl1pnspIs/AQk/KqA9lBd+kuKyVTiE4uPAoGBAPQYFFroalVEwaYtKv4Zz5gT+Y2OB5ZjJ5bmmpVQnwZevA/b+EOZHoeJsDN87HGAhtRTNCcnyyHVNx7VxTPegnAmDFrOcjiYpTA5Jat3swtHD9rhoD/5I8eHg0B282MTHplWQ7S1YuD+C4IJ1GmER9vz8EAey4NLurbJHMLPPXcTAoGBAN4dgPTadZtya4dTJpGBN0KffLzcu8vOY8kFHbwKvY778L00q//GjYucLVgsW3UkbxTqjCHxQf9qeMojXcBmjRs6kZ/JYNpS4jYvZ3Yf+Jx1VSSrLYbWGY+tBO/zVabyvGe6dbjSgyRBnGdV/22LgX4vSNJxfDjFwotOMfYpdFpPAoGBAJ0apndAYbgR8eWH6sTzTebe7F7MjFuh5Ag+0j3KTyAldztp4+d2NO0dlLf+7pu7Eoy0JLwS464Z8kk5Y5yX2TJfetIzT9bXgHKCRZmQCD/48954G9ExBHNW6AZkyQ/6bVZZ7Gfx2vte539B3mIvjqSl4/sRFwGhi0I8PPOxuSjbAoGBAJJxL1nAQvQXL1AMgYxDfnSdhHdcrTCFRgL+LYmSJ0KDV0jX1mMPvLkEYl0U+cO1HsvSjEvArfvBbhwPzzsQIg6GwgZwlju7k8uX24XlNFurfFRty9lvhXDV6UBu1dT5i0B0jjMqEC6yV3VGHN9TsC/K6x9clUM1F7wS9RvQXxc/AoGAH6FIsmLNDJkwOntyy/XayRbyDAf6oktVvjRlRoUDhwaKzgCqeZ601gJT6naO8Jovh30wLbSHrflhM2oy8Wyi0dknGnPHm0QifmI+JSYMBVwS267j8OcDROWJ04zHbv20jgi5y8myV7jzbnU01MUyhoOiX1f3gU7OzkKKqcG4IPU=
                 */
                
                 /*

                 
                string open = ImportPublicKey(op).ToXmlString(false);
                string close = ImportPrivateKey(pop).ToXmlString(true);
                //close = "<RSAKeyValue><Modulus>pajps6oKJS3uFbto/B3OXQtNzf8SnVK60AcYq5oUAwKU+lf06xg3EtZIl2crZRiG6goR5XWHIwFFGjF0iWTyLm/KxEwdlK1xlfA7O2GEnA8FqCkql91g5XPaUE7aLirKPr+1Dq/pjoU24csb4793PxLVz04UQJ/C9ydZYXiBLrzTL+TQ2S+Cr2a9kklBBL2FB2ortz0aX3xxojZLLzO8odZhPdxqN13pXNx3+h6Z/WBMnDQ3GdQ6BaX16W9R/PoUNRWiMZY9Mq/RZlzh2jPTMqIyduguM2IowMQ2Jxs5gRxzuTtv1zHjp+rdAoQ2F7lovkvcAEgbAFKn5kBltp+9cQ==</Modulus><Exponent>AQAB</Exponent><P>w7bggL3v0ha4e+GYSyniinVEUZsFekoF/P28st+A3l6ZEvJcXfKPJLZbVuq15BN9wKfDRrGe5CK36bqpgHjruX7dssDAdDILFs+DMm0q7A4ueI7QOv338F5jABP1Pq56lOjXEEQoCQNEKLrCxyuRqJemDCRAZqvPRojQLBXdZUs=</P><Q>2LATCAErEr0IkjKWquagGCepDMfTOw9mEvxpKXTOUIcGMb4xyVXzMU66EXcqVh+OVmrQQklQKMXhw9c9wHvDBJglLU4QaowzLq6I4Uz2U3I2s+lm2cDorx+QJwT1hwK3pzJ935RBspzgyWinZAGGD3e1o1GWN8vGyst1uV+FfrM=</Q><DP>G7sf2F4RSw1ZGoR+lbfbl47CHWX/wrIWYyB4ykeM9PUmb17T46Po0GTeUFR1bVmtqcybiLrGLsEUmhOOzNtVWzU0qI1TN6OXmqXQiyaYvFXOE9r4ekNeDDrzgCFT0IvcZpkPUAi4O36w+6xqyNnNi7vhFTtVvLZ/ahkclgKLGj0=</DP><DQ>j40uPnpPp81V1UzwZe39l6MGqnHjbVgJPoC7xnwchlq9bszavNBVGZBXUmTIxl+Hc5a1u4RIN4rIw25f+ZnEG9ITpwz1cTDKY527Ds9mzOd4d/4jZhUX0ZSucjsl0biqJHkdzLWyRHsLrELJykX8I1kiGz1csEAuxYC+z/0JOmM=</DQ><InverseQ>jMsB9vvu592Uy3QykYq9gmjdJGD7HBWug71ARz/Z7TYYgsWS7h7v4vo1FYULqAyxfh7MZNYouy3byEdWzXYrZUOasiVDcw54V34Yqwib7kgdqClM/vk+RcH5gMARVoH/XTfEEeKLtANY8VVkC+EelcZ6qqbdLuNVAenBhD7/aVQ=</InverseQ><D>BiphvFPv+bnvul5VxiBj3axTMn9eVX2j7KR++Lq3fknaept1xvQz7zRg0kW+5I3Y+PKeqjrJaKDzo0nDlm4r4f+Q0o/Uqq5ujk/jxyOA2IAUoVFwP45ikgJRq0XWsqYuuHl7xyFT9ip4xEbsxvlez6RR/IZhlEAon6NBcofAzcOeV988ZEAahKIc2dAntFb8C048JCUdFMXc+fDWog3578819apmglZTcbkRGFpQnWAB+BkuByysv6qFMhYjMCWsUwU2tYLsBOB0U8ACKUkXtygJ8OntfRu/HJNZq0OxY8PKBctI6JYm1eyPVSC2gaIP8zWG1YZAxiQNZJR9W9rkaw==</D></RSAKeyValue>";
                string todecrypt = "kLjgYVyMgBb0PTqJBzKDm0am28TU8KxSCQkifTf4Fj9ROLHMUPZWANdyavU4ZXMt9CXrMDLqr2tckI2wbDC3v6mhJ8tX773GewH5bx9nD5KY9GaaQ1RxDedAyTjPp2Sdp1D+FyUf7aghKWX1ArojlRNXzbfNKW87T/8KfNh7FVbHeIklm4uK/5Mim5NK1usGKRcEV2L7SFi+m7K5CG4o3u3c/SxM9g9AHfGQb24bUbUY2XXAMbHhTyiXCdMpyqZtSnuzjso+z0pPDqd61sMDxBeSiHBv2dnrBp7HS0oyJomN97MXIv8X/a3qm0DBR9zBCOZTu5VFqp/9uW01SWLV5g==";
                richTextBox3.Text += decrypt11(todecrypt, close);
                

                */


                //richTextBox3.Text += decript(BytesToString(Encoding.UTF8.GetBytes(todecrypt)), close);

                RSACryptoServiceProvider RsaKey = new RSACryptoServiceProvider();
                richTextBox1.Text += ExportPublicKey(RsaKey) + "\n";
                richTextBox1.Text += ExportPrivateKey(RsaKey);

                string _pass = encrypt("Heloo world", RsaKey.ToXmlString(false));
                richTextBox1.Text += "\n"+Convert.ToBase64String(StringToBytes(_pass)) + "\n";
                richTextBox1.Text += _pass;
                
                
                
                /*
                string open = ImportPublicKey(op).ToXmlString(false);
                string close = ImportPrivateKey(pop).ToXmlString(true);

                string gk = "VWuqWHvpaMOQ6mPfLfRVlyQXSFbuowe+doVlTqXrdQlIphVkUnj7ZMVVRa1SgP6KLCL6EnQa5oFXxY6Nq7/DoI91jkBcyC7Qx+k94wT+klaBmruO0tFBEkN2VPTLbURtrZTU2Xx6xJ44rr/UNgpPUS8iICMuaREbl1av5snJj3iUsZuQIa20GbH28EUM4cb1Vy7ra6pQ5/pZL0dHOfBefoF1dt1/x9ODqnzlcrTWrEm5x1FQS99qMa9ja7FMCcIeLCRAgn7YUPCC/HhY3fuOXK9LV99H1D4FmEl/MT0S22W9ClLpvE0sZr5zsz1MWsO4zIixHR63I/J6aZSnlOs0lQ==";
                richTextBox1.Text += BytesToString(Encoding.UTF8.GetBytes(gk));
                string _pass = encript("Heloo world", open);
                richTextBox1.Text += "";

                richTextBox1.Text += Convert.ToBase64String(StringToBytes(encript("hello s", open)));

                richTextBox1.Text += decript("86|87|117|113|87|72|118|112|97|77|79|81|54|109|80|102|76|102|82|86|108|121|81|88|83|70|98|117|111|119|101|43|100|111|86|108|84|113|88|114|100|81|108|73|112|104|86|107|85|110|106|55|90|77|86|86|82|97|49|83|103|80|54|75|76|67|76|54|69|110|81|97|53|111|70|88|120|89|54|78|113|55|47|68|111|73|57|49|106|107|66|99|121|67|55|81|120|43|107|57|52|119|84|43|107|108|97|66|109|114|117|79|48|116|70|66|69|107|78|50|86|80|84|76|98|85|82|116|114|90|84|85|50|88|120|54|120|74|52|52|114|114|47|85|78|103|112|80|85|83|56|105|73|67|77|117|97|82|69|98|108|49|97|118|53|115|110|74|106|51|105|85|115|90|117|81|73|97|50|48|71|98|72|50|56|69|85|77|52|99|98|49|86|121|55|114|97|54|112|81|53|47|112|90|76|48|100|72|79|102|66|101|102|111|70|49|100|116|49|47|120|57|79|68|113|110|122|108|99|114|84|87|114|69|109|53|120|49|70|81|83|57|57|113|77|97|57|106|97|55|70|77|67|99|73|101|76|67|82|65|103|110|55|89|85|80|67|67|47|72|104|89|51|102|117|79|88|75|57|76|86|57|57|72|49|68|52|70|109|69|108|47|77|84|48|83|50|50|87|57|67|108|76|112|118|69|48|115|90|114|53|122|115|122|49|77|87|115|79|52|122|73|105|120|72|82|54|51|73|47|74|54|97|90|83|110|108|79|115|48|108|81|61|61", close);


                RSACryptoServiceProvider RsaKey = new RSACryptoServiceProvider();
                richTextBox1.Text += ExportPublicKey(RsaKey)+"\n";
                richTextBox1.Text += ExportPrivateKey(RsaKey);*/
            }




            //richTextBox3.Text += GetMessage(textBox1.Text, textBox2.Text, openkey, richTextBox1);
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
            string answer = GetResponseString(uri + "getconfurm", json);
            string serverTokenEncrypted = answer.Split(':')[1].Trim('}').Trim('"');
            string serverToken = decrypt(serverTokenEncrypted, privateKey);
            return serverToken.Split('|')[1];
        }

        public static string Registration(string uri, string name, string pass, string openkey, string closekey, RichTextBox richTextBox)
        {
            string _myToken = Guid.NewGuid().ToString();
            string json = "";
            string _openkey = GetResponseString(uri + "getkeyxml", json).Split(':')[1].Trim('}').Trim('"');
            string _serverToken = GetConfurm(uri, name, _myToken, 0, _openkey, openkey, closekey);

            string BaseUri = uri + "registration";
            string _name = encrypt(_serverToken + "|" + name + "|" + _myToken, _openkey);
            string _pass = encrypt(_myToken + "|" + pass + "|" + _serverToken, _openkey);
            json = "{" + $"\"Name\":\"{_name}\", \"openkey\":\"{openkey}\", \"Password\":\"{_pass}\"" + "}";
            string answer = GetResponseString(BaseUri, json);
            string EnId = answer.Substring(9).TrimEnd('}').Trim('"');

            return decrypt(EnId, closekey).Split('|')[1];
        }

        public static string Authorization(string uri, string name, string pass, string openkey, string closekey, RichTextBox richTextBox)
        {
            string _myToken = Guid.NewGuid().ToString();
            string json = "";
            string _openkey = GetResponseString(uri + "getkeyxml", json).Split(':')[1].Trim('}').Trim('"');
            string _serverToken = GetConfurm(uri, name, _myToken, 1, _openkey, openkey, closekey);

            string BaseUri = uri + "authorization";

            string _name = encrypt(_serverToken + "|" + name + "|" + _myToken, _openkey);
            string _pass = encrypt(_myToken + "|" + pass + "|" + _serverToken, _openkey);
            json = "{" + $"\"Name\":\"{_name}\", \"openkey\":\"{openkey}\", \"Password\":\"{_pass}\"" + "}";
            string answer = GetResponseString(BaseUri, json);
            string EnId = answer.Substring(9).TrimEnd('}').Trim('"');

            return decrypt(EnId, closekey).Split('|')[1];
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
            string _name = encrypt(myname, _openkey);
            json = "{" + $"\"Name\":\"{_name}\", \"openkey\":\"{openkey}\"" + "}";
            string answer = GetResponseString(BaseUri, json);
            richTextBox.Text += answer + "\n";
            return answer;
        }

        public static string GetInformAboutSendedMessages(string uri, string myname, string openkey, RichTextBox richTextBox)
        {
            string BaseUri = uri + "checkMessagesInfo";
            string json = "";
            string _openkey = GetResponseString(uri + "getkey", json).Split(':')[1].Trim('}').Trim('"');
            string _name = encrypt(myname, _openkey);
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
    }
}
