using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace PuzzleAttack5000 {

    class MapArray {
        const int ALPHABET_LENGTH = 26;
        const int MIX_COUNT = 129;

        private char[] _alphabet = null;
        private char[] _alphabetcopy = null;
        private string _phrase = "";
        private string _EncodedPhrase = "";



        public MapArray() {
            _alphabet = new char[ALPHABET_LENGTH];
            char AsciiA = (char)65;

            // Generate the alphabet
            for (int i = 0; i < _alphabet.Length; i++) {
                _alphabet[i] = (char)(AsciiA + i);
            }

            _alphabetcopy = _alphabet.ToArray();
            Jumble();
        }

        public String Phrase {
            set { _phrase = value.ToUpper(); }
            get { return _phrase; }
        }

        public string EncodedPhrase {
            get { return _EncodedPhrase; }
            set { _EncodedPhrase = value; }
        }

        public String Key {
            get { return new String(_alphabet); }
        }

        public String Alphabet {
            get { return new String(_alphabetcopy); }
        }

        private void Jumble() {
            Random Rand = new Random();
            for (int i = 0; i < MIX_COUNT; i++) {
                char TempSource = (char)0;
                char TempDestination = (char)0;
                int SourcePosition = Rand.Next(ALPHABET_LENGTH);
                int DestinationPosition = Rand.Next(ALPHABET_LENGTH);
                while (SourcePosition == DestinationPosition) {
                    DestinationPosition = Rand.Next(ALPHABET_LENGTH);
                }
                TempSource = _alphabet[SourcePosition];
                TempDestination = _alphabet[DestinationPosition];
                _alphabet[SourcePosition] = TempDestination;
                _alphabet[DestinationPosition] = TempSource;
            }

            // Writer.WriteLine(new String(_alphabet));
            // Writer.WriteLine(new String(_alphabetcopy));

            bool PairMatchFound = true;
            while (PairMatchFound) {
                int j = 0;
                PairMatchFound = false;
                while (j <= ALPHABET_LENGTH - 1) {
                    int Start = j;
                    int End = (ALPHABET_LENGTH - j) - 1;
                    // Writer.WriteLine("   " + _alphabet[j] + " - " + _alphabetcopy[j]);
                    if (_alphabet[j] == _alphabetcopy[j]) {
                        // Writer.WriteLine("==>" + _alphabet[j] + " = " + _alphabetcopy[j]);
                        char TempChar = _alphabet[End];
                        _alphabet[End] = _alphabet[Start];
                        _alphabet[Start] = TempChar;
                        PairMatchFound = true;
                    }
                    j++;
                }
            }
        }
        public void EncodePhrase() {
            foreach (char c in _phrase) {
                if (Char.IsLetter(c)) {
                    int index = 0;
                    bool FoundChar = false;
                    while (!FoundChar) {
                        if (c.Equals(_alphabetcopy[index])) {
                            _EncodedPhrase += _alphabet[index];
                            FoundChar = true;
                        }
                        else {
                            index++;
                        }
                    }
                }
                else {
                    _EncodedPhrase += c;
                }
            }
        }

    }

    class Program {
        static void Main(string[] args) {
            Random rnd = new Random();
            Random rnd2 = new Random();
            int num1 = rnd.Next(0, 3);
            int num2 = rnd.Next(0, 3);
            //if (num1 == num2) {
            //Console.Write("Enter a message, then press enter: ");
            //string Phrase = Console.ReadLine();
            //string Phrase = GetVerseOfTheDay();
            string Phrase = GetQuoteOfTheDay();
            if (!Phrase.Equals("")) {
                string FileName = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                string path = Path.Combine("C:\\puzzles\\", FileName + "_quote.txt");
                StreamWriter Writer = File.CreateText(path);

                Console.Clear();
                MapArray Code = new MapArray();
                Writer.WriteLine("-----------KEY------------");
                Writer.WriteLine();
                Writer.WriteLine(Code.Alphabet);
                Writer.WriteLine(Code.Key);
                Code.Phrase = Phrase;
                Code.EncodePhrase();

                Writer.WriteLine();
                Writer.WriteLine("---------MESSAGE----------");
                Writer.WriteLine();

                Writer.WriteLine(Code.Phrase);
                Writer.WriteLine(Code.EncodedPhrase);
                Writer.WriteLine();
                Writer.Close();

                WriteEmail(Code.EncodedPhrase);
            }
            //}

            //WriteSolutionEmail(Code.EncodedPhrase, FileName);
            //Writer.WriteLine("Press any key to exit...");
            //Writer.ReadKey();
        }

        public static void WriteErrorMail(string ErrorMessage) {
            MailMessage Message = new MailMessage();
            Message.From = new MailAddress("mcooksey@csa1.com", "Puzzle Attack 5010");

            Message.Subject = "Quote Puzzle Failure";
            Message.Body = ErrorMessage;
            Message.To.Add("mcooksey@csa1.com");

            MailCredentials Credentials = new MailCredentials();

            SmtpClient Client = new SmtpClient();
            Client.Host = Credentials.Host;
            Client.Credentials = new NetworkCredential(Credentials.UserName, Credentials.Password);
            Client.Send(Message);
        }

        private static void WriteEmail(string EncodedPhrase) {
            MailMessage Message = new MailMessage();
            Message.From = new MailAddress("mcooksey@csa1.com", "Puzzle Attack 5010");

            Message.Subject = "Quote of the Day Puzzle!";
            Message.Body = EncodedPhrase;


            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emails.xml"));
            XmlNodeList Addresses = doc.SelectNodes("PuzzleAttackSettings/Addresses/address");
            foreach (XmlNode address in Addresses)
                Message.To.Add(address.InnerText);

            MailCredentials Credentials = new MailCredentials();

            SmtpClient Client = new SmtpClient();
            Client.Host = Credentials.Host;
            Client.Credentials = new NetworkCredential(Credentials.UserName, Credentials.Password);
            Client.Send(Message);
        }

        private static void WriteSolutionEmail(string EncodedPhrase, string FileName) {
            MailMessage Message = new MailMessage();
            Message.From = new MailAddress("mcooksey@csa1.com");

            Message.Subject = "Quote of the Day Puzzle!";
            Message.IsBodyHtml = true;
            Message.Body = "<html><body><font size=\"3\" face=\"arial\">" + EncodedPhrase + "</font>"
                       + "<br><br><font size=\"1\" face=\"arial\"><i>Click <a href=\"10.1.150.174:8080/" + FileName + ".txt\">here</a> to see the solution.</i></font></body></html>";
            Message.To.Add("mcooksey@csa1.com");

            MailCredentials Credentials = new MailCredentials();

            SmtpClient Client = new SmtpClient();
            Client.Host = Credentials.Host;
            Client.Credentials = new NetworkCredential(Credentials.UserName, Credentials.Password);
            Client.Send(Message);
        }

        private static string GetQuoteOfTheDay() {
            string ReturnValue = "";
            int Attempts = 0;
            while (Attempts <= 3 && ReturnValue.Equals("")) {
                try {
                    XNamespace content = "URI";
                    StringBuilder sb = new StringBuilder();
                    string Url = "http://archive.aweber.com/qod.rss";
                    XmlReader Reader = XmlReader.Create(Url);
                    SyndicationFeed Feed = SyndicationFeed.Load(Reader);
                    SyndicationItem Item = Feed.Items.First();
                    string Quote = Item.Summary.Text.Replace("\n", "");
                    int EndOfQuote = Quote.LastIndexOf("LIKE THIS QUOTE?");
                    Quote = Quote.Substring(0, EndOfQuote);
                    ReturnValue = Quote;
                    Reader.Close();
                } catch (Exception Ex) {
                    if (Attempts == 0) {
                        WriteErrorMail("Unable to retrieve verse: \n" + Ex.Message + "\n" + Ex.StackTrace);
                    }
                    Attempts++;
                }
            }

            return ReturnValue;

        }

        private static string GetVerseOfTheDay() {
            string ReturnValue = "";
            int Attempts = 0;
            while (Attempts <= 3 && ReturnValue.Equals("")) {
                try {
                    XNamespace content = "URI";
                    StringBuilder sb = new StringBuilder();
                    string url = "https://www.biblegateway.com/usage/votd/rss/votd.rdf?9";
                    XmlReader reader = XmlReader.Create(url);
                    SyndicationFeed feed = SyndicationFeed.Load(reader);

                    reader.Close();
                    foreach (SyndicationItem item in feed.Items) {

                        foreach (SyndicationElementExtension extension in item.ElementExtensions) {
                            XElement ele = extension.GetObject<XElement>();
                            if (ele.Name.LocalName == "encoded" && ele.Name.Namespace.ToString().Contains("content")) {
                                sb.Append(ele.Value + "<br/>");
                            }
                        }
                    }
                    string Verse = sb.ToString();
                    int pFrom = Verse.IndexOf("&ldquo;") + "&ldquo;".Length;
                    int pTo = Verse.LastIndexOf("&rdquo");
                    ReturnValue = Verse.Substring(pFrom, pTo - pFrom);
                } catch (Exception Ex) {
                    if (Attempts == 0) {
                        WriteErrorMail("Unable to retrieve verse: \n" + Ex.Message + "\n" + Ex.StackTrace);
                    }
                    Attempts++;
                }
            }

            return ReturnValue;
        }
    }

    class MailCredentials {
        private string _Host = "";

        public string Host {
            get { return _Host; }
            set { _Host = value; }
        }

        private string _UserName = "";

        public string UserName {
            get { return _UserName; }
            set { _UserName = value; }
        }

        private string _Password = "";

        public string Password {
            get { return _Password; }
            set { _Password = value; }
        }

        public MailCredentials() {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emails.xml"));
            XmlNode host = doc.SelectSingleNode("PuzzleAttackSettings/EmailSettings/host");
            this.Host = host.InnerText;

            XmlNode user = doc.SelectSingleNode("PuzzleAttackSettings/EmailSettings/user");
            this.UserName = user.InnerText;

            XmlNode password = doc.SelectSingleNode("PuzzleAttackSettings/EmailSettings/password");
            this.Password = password.InnerText;
        }
    }
}

