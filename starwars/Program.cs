using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections;
using System.Web.Script.Serialization;
using StarWarsAPI;
using StarWarsAPI.Model;
using System.Text.RegularExpressions;

namespace starwars
{
    class Program
    {

        static void Main(string[] args)
        { /*
            Utilizando a StarWars APi helper 
            passos: determinar o total das naves (count)
            -verificar dinamicamente quantas paginas existem no arquivo
            -retornar os dados (result) em um List
            - calcular o numero de paradas
            - exibir o List
                       
             */

            var api = new StarWarsAPIClient();
            int count; // Total de registros
            bool proximo; // se contém próxima página
            int naves_por_pagina;


            Console.Write("Entre a distância desejada em MGLT: ");

            Globals.MGLT = Console.ReadLine();

            List<Nave> Ship = new List<Nave>();

            int indice = 1;
            Console.Write("Buscando Dados das Naves...\n\n");
            Console.WriteLine(" ]|| ---INICIO DA TRANSMISSAO-- -||[ ");
            Nave novanave = new Nave();
            do
            {
                StarWarsEntityList<Starship> nave = api.GetAllStarshipAsync(indice.ToString()).Result;
                count = Convert.ToInt32(nave.count);
                proximo = nave.isNext;
                naves_por_pagina = nave.results.Count();

                foreach (var navinha in nave.results)
                {
                    novanave.Name = navinha.name;
                    novanave.MGLT = navinha.MGLT;
                    novanave.Consumables = navinha.consumables;
                    novanave.ResupplyFrequency = CalculeResupplyFrequency(novanave);
                    if (novanave.ResupplyFrequency == "unknown") { novanave.ResupplyFrequency = "indeterminado"; };

                    Ship.Add(novanave);

                    Console.WriteLine("Nave: " + novanave.Name + " : Número de Paradas : " + novanave.ResupplyFrequency);
                }
                indice++;
                
            } while (proximo == true) ;

            Console.WriteLine("||--- FIM DA TRANSMISSAO ---||\n\n");
            Console.WriteLine("PRESSIONE QUALQUER TECLA PARA FINALIZAR...");
            Console.ReadLine();

            }

        public static string CalculeResupplyFrequency(Nave starship)
        {
            //Function From https://github.com/scharamoose/

            if (!Regex.IsMatch(starship.MGLT, @"^\d+$"))
            {
                return starship.MGLT;
            }

            DateTime JourneyStart = DateTime.Now;
            DateTime JourneyEnd = DateTime.Now;

            string[] tokenisedProvisionsOfConsumables = starship.Consumables.Split(' ');

            switch (tokenisedProvisionsOfConsumables[1].ToLower())
            {
                case "day":
                case "days":
                    JourneyEnd = JourneyEnd.AddDays(Convert.ToDouble(tokenisedProvisionsOfConsumables[0]));
                    break;
                case "week":
                case "weeks":
                    JourneyEnd = JourneyEnd.AddDays(Convert.ToDouble(tokenisedProvisionsOfConsumables[0]) * 7);
                    break;
                case "month":
                case "months":
                    JourneyEnd = JourneyEnd.AddMonths(Convert.ToInt32(tokenisedProvisionsOfConsumables[0]));
                    break;
                case "year":
                case "years":
                    JourneyEnd = JourneyEnd.AddYears(Convert.ToInt32(tokenisedProvisionsOfConsumables[0]));
                    break;
            }

            int minutes = (int)(JourneyEnd - JourneyStart).TotalMinutes;
            int numHours = Enumerable.Range(0, minutes)
                .Select(min => JourneyStart.AddMinutes(min))
                // Arredondar para hora, já que MGLT é a velocidade por hora. 
                .GroupBy(dt => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0))
                .Count();

            return (Convert.ToInt64(Globals.MGLT) / (numHours * Convert.ToInt64(starship.MGLT))).ToString();
        }
    }
}
