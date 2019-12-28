using IpScanner;
using System;
using System.Collections.Generic;

namespace Imieniny
{
    class ProcessNameDayDataClass
    {
        public static void NamesInfo(string data, ref string header, ref string toShowData)
        {
            header = "• " + data.Substring(data.IndexOf("<h1>Imieniny ") + 4).Substring(0, data.Substring(data.IndexOf("<h1>Imieniny ") + 4).IndexOf("</h1>")) + ": \n\n";

            string imionaBegin = data.Substring(data.IndexOf("class=\"main_imi\"") + 47);
         
            imionaBegin = imionaBegin.Substring(0, imionaBegin.IndexOf("</tr>"));

            string[] imiona = imionaBegin.Split(new string[] { "<a href=\"/" }, StringSplitOptions.None);

            for (int i = 1; i < imiona.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(imiona[i]))
                {
                    imiona[i] = imiona[i].Substring(0, imiona[i].IndexOf("\""));
                    toShowData += ("- " + imiona[i] + new string(' ', 5));
                }
            }
        }

        public static void DayInfo(string data, ref string toShowData)
        {
            toShowData = "\n";

            string dayData = data.Substring(data.IndexOf("<p><b>") + 6);

            dayData = dayData.Substring(0, dayData.IndexOf("</p>"));

            dayData = dayData.Replace("</b>", string.Empty).Replace("<br />", string.Empty).Replace("  ", string.Empty).Replace(".", ",|");

            string[] tempDayData = dayData.Split('|');

            for (int i = 0; i < tempDayData.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(tempDayData[i]))
                {
                    tempDayData[i] = tempDayData[i].Trim();
                    if (i == tempDayData.Length - 2) tempDayData[i] = tempDayData[i].Replace(",", string.Empty);
                    toShowData += (tempDayData[i] + '\n');
                }
            }
        }

        public static void DayInfo2(string data, ref string toShowData)
        {
            toShowData = null;

            string tempDayData2 = data.Substring(data.IndexOf(" </div><div class=\"box\"> <p>") + 28);
            tempDayData2 = tempDayData2.Substring(0, tempDayData2.IndexOf("</p>"));
            tempDayData2 = tempDayData2.Replace("<br /", string.Empty);

            string[] dayData2 = tempDayData2.Split('>');

            for (int i = 0; i < dayData2.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(dayData2[i]))
                {
                    if (i == 0) toShowData += ("- " + dayData2[i] + "\n");
                    else toShowData += ('-' + dayData2[i] + '\n');
                }
            }
        }

        public static void SaintsInfo(string data, ref string header, ref string toShowData)
        {
            toShowData = null;

            try
            {
                string tempSaints = data.Substring(data.IndexOf("<h2>") + 4);

                string titleSaints = tempSaints.Substring(0, tempSaints.IndexOf("</h2>"));

                header = "• " + titleSaints + "\n";

                tempSaints = tempSaints.Substring(tempSaints.IndexOf("<li>") + 4);

                string[] saints = tempSaints.Split(new string[] { "</li><li>" }, StringSplitOptions.None);

                saints[saints.Length - 1] = saints[saints.Length - 1].Substring(0, saints[saints.Length - 1].IndexOf("</li>"));

                toShowData += '\n';

                foreach (var s in saints) toShowData += ("- " + s + '\n');
            }
            catch (Exception) { }
        }

        public static void PolandEventsInfo(string data, ref string header, ref string toShowData)
        {
            toShowData = null;

            List<Events> polandEventsList = new List<Events>();

            try
            {
                string tempPolandEvents = data.Substring(data.IndexOf("<h3>") + 4); //Od miejsca zaczecia do konca kodu strony
                tempPolandEvents = tempPolandEvents.Substring(0, tempPolandEvents.IndexOf("colspan=\"2\"")); // od miejsca zaczecia do konca wydarzen

                string titlePolandEvents = tempPolandEvents.Substring(0, tempPolandEvents.IndexOf("</h3>")); // tytul naglowka "Wydarzenia w polsce"

                header = "• " + titlePolandEvents + ": \n\n";

                tempPolandEvents = tempPolandEvents.Substring(tempPolandEvents.IndexOf("<td>") + 4); //usuwanie tytulu

                tempPolandEvents = tempPolandEvents.Replace("</td></tr><tr><td", string.Empty).Replace("&nbsp;-</td><td>", "|").Replace("</td></tr><tr><td", string.Empty); //Porzadkowanie w celu wyodrebnienia daty i nazwy

                string[] eventData = tempPolandEvents.Split('>'); //rodzielanie na pojedyncze wydarzenia + daty

                for (int i = 0; i < eventData.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(eventData[i]))
                    {
                        string[] pom = eventData[i].Split('|');
                        try { polandEventsList.Add(new Events { Year = pom[0], EventName = pom[1] }); }
                        catch (Exception) { }
                    }
                }

                foreach (var s in polandEventsList) toShowData += (s.Year + ": " + s.EventName + '\n');
            }
            catch (Exception) { }
        }

        public static void WorldEventsInfo(string data, ref string header, ref string toShowData)
        {
            toShowData = null;

            List<Events> worldEventsList = new List<Events>();

            try
            {
                string tempWorldEvent = data.Substring(data.IndexOf("Wydarzenia na świecie")); // Od miejsca zaczecia do konca kodu strony
                tempWorldEvent = tempWorldEvent.Substring(0, tempWorldEvent.IndexOf("colspan=\"2\"")); // od miejsca zaczecia do konca wydarzen

                string titleWorldEvents = tempWorldEvent.Substring(0, tempWorldEvent.IndexOf("</h3>")); // tytul naglowka "Wydarzenia na swiecie"

                header = "• " + titleWorldEvents + ": \n\n";

                tempWorldEvent = tempWorldEvent.Substring(tempWorldEvent.IndexOf("<td>") + 4); //usuwanie tytulu

                tempWorldEvent = tempWorldEvent.Replace("</td></tr><tr><td", string.Empty).Replace("&nbsp;-</td><td>", "|").Replace("</td></tr><tr><td", string.Empty); //Porzadkowanie w celu wyodrebnienia daty i nazwy

                string[] worldEventData = tempWorldEvent.Split('>'); //rodzielanie na pojedyncze wydarzenia + daty

                for (int i = 0; i < worldEventData.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(worldEventData[i]))
                    {
                        string[] pom = worldEventData[i].Split('|');
                        try { worldEventsList.Add(new Events { Year = pom[0], EventName = pom[1] }); }
                        catch (Exception) { }
                    }
                }

                foreach (var s in worldEventsList) toShowData += (s.Year + ": " + s.EventName + '\n');
            }
            catch (Exception) { }
        }

        public static void PersonBornInfo(string data, ref string header, ref string toShowData)
        {
            toShowData = null;

            List<PersonBorn> personList = new List<PersonBorn>();

            try
            {
                string tempPerson = data.Substring(data.IndexOf("Osoby urodzone ")); // Od miejsca zaczecia do konca kodu strony
                tempPerson = tempPerson.Substring(0, tempPerson.IndexOf("</table>")); // od miejsca zaczecia do konca wydarzen

                string titlePerson = tempPerson.Substring(0, tempPerson.IndexOf("</h3>")); // tytul naglowka "Osoby urodzone w "

                header = "• " + titlePerson + ": \n\n";

                tempPerson = tempPerson.Substring(tempPerson.IndexOf("<td>") + 4); //usuwanie tytulu

                tempPerson = tempPerson.Replace("</td></tr><tr><td", string.Empty).Replace("&nbsp;-</td><td>", "|").Replace("</td></tr>", string.Empty); //Porzadkowanie w celu wyodrebnienia daty i nazwy

                string[] personData = tempPerson.Split('>'); //rodzielanie na pojedyncze nazwiska + daty

                for (int i = 0; i < personData.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(personData[i]))
                    {
                        string[] pom = personData[i].Split('|');
                        personList.Add(new PersonBorn { Year = pom[0], Name = pom[1] });
                    }
                }

                foreach (var s in personList) toShowData += (s.Year + ": " + s.Name + '\n');
            }
            catch (Exception) { }
        }
    }
}