using System.Net;
using System.Net.Sockets;

namespace IpScanner
{ 
    public class GetIp
    {
        public static string Subnet() //Adres IP hosta
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }

       public static int IndexOfNth(string str, string value, int nth) //Szukam gdzie jest 3 kropka w adresie ip 192.168.1.
        {
            int offset = str.IndexOf(value);
            for (int i = 1; i < nth; i++)
            {
                if (offset == -1) return -1;
                offset = str.IndexOf(value, offset + 1);
            }
            return offset;
        }

        public static string Result() => Subnet().Substring(0, IndexOfNth(Subnet(), ".", 3));
    }
}
/*
 * Ta klasa pobiera adres ip hosta np. 192.168.1.14
 * Następnie przerabia go na: 192.168.1.
 * Aby podczas pingowania adresow IP w petli mozna dodawać po kolei nastepna wartosc np:
 * 192.168.1.1, 192.168.1.2
 * Korzysta sie z niej jak uzytkownik bedzie chcial wyszkukac na podstawie wlasnego adresu ip
 * Zawartosc ipTextBox.text jest brana wtedy na podstawie danych z tej klasy
 */