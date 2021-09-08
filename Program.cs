using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JsonParser
{
	class Program
	{
		public static void Main(string[] args)
		{

			string inputFilePath = args.Length>=1 ? args[0] : string.Empty;
			string outputFilePath = args.Length >= 2 ? args[1] : string.Empty;

			if (string.IsNullOrEmpty(inputFilePath))
			{
				Console.WriteLine("No argument detected.");
				return;
			}

			if (!File.Exists(inputFilePath) || !Path.GetExtension(inputFilePath).Contains("json"))
			{
				Console.WriteLine("No file detected.");
				return;
			}

			JToken jToken;

			using (StreamReader inputFile = File.OpenText(inputFilePath))
			using (JsonTextReader reader = new JsonTextReader(inputFile))
			{
				jToken = JToken.ReadFrom(reader);
			}

			var values = jToken
					.SelectTokens("$..*")
					.Where(t => !t.HasValues)
					.ToDictionary(t => t.Path, t => t.ToString());

			var itemNumber = values.Keys.Select(n => Regex.Match(n, @"item\[\d+\]").Value).Distinct();

			List<Item> itemList = new List<Item>();
			List<Batter> batterList = new List<Batter>();
			List<Toppings> toppingList = new List<Toppings>();

			foreach (string number in itemNumber)
			{
				Item item = new Item();
				item.Id = values.Where(n => !string.IsNullOrEmpty(Regex.Match(n.Key, string.Format(@"{0}\.id", Regex.Escape(number))).Value)).Select(n=>n.Value).FirstOrDefault();
				item.Type = values.Where(n => !string.IsNullOrEmpty(Regex.Match(n.Key, string.Format(@"{0}\.type", Regex.Escape(number))).Value)).Select(n => n.Value).FirstOrDefault();
				item.Name = values.Where(n => !string.IsNullOrEmpty(Regex.Match(n.Key, string.Format(@"{0}\.name", Regex.Escape(number))).Value)).Select(n => n.Value).FirstOrDefault();

				itemList.Add(item);

				foreach (string batterType in values.Where(n => !string.IsNullOrEmpty(Regex.Match(n.Key, string.Format(@"{0}[.\w]*.batter\[\d+\]\.type", Regex.Escape(number))).Value)).Select(n => n.Value))
				{
					Batter batter = new Batter();
					batter.Parent = item;
					batter.Type = batterType;

					batterList.Add(batter);
				}

				foreach (string toppingType in values.Where(n => !string.IsNullOrEmpty(Regex.Match(n.Key, string.Format(@"{0}[.\w]*.topping\[\d+\]\.type", Regex.Escape(number))).Value)).Select(n => n.Value))
				{
					Toppings topping = new Toppings();
					topping.Parent = item;
					topping.Type = toppingType;

					toppingList.Add(topping);
				}
			}

			var query = from item in itemList
									join batter in batterList on item equals batter.Parent into bi
									from subbatter in bi.DefaultIfEmpty()
									join topping in toppingList on item equals topping.Parent into ti
									from subtopping in ti.DefaultIfEmpty()
									select new PrintRow {Id = item.Id, Type = item.Type, Name = item.Name, Batter = subbatter?.Type ?? string.Empty, Topping = subtopping?.Type ?? string.Empty };

			if (string.IsNullOrEmpty(outputFilePath))
			{
				Console.WriteLine("Id \t Type \t Name \t Batter \t Topping");
				foreach (PrintRow pr in query)
				{
					Console.WriteLine($"{pr.Id} \t {pr.Type} \t {pr.Name} \t {pr.Batter} \t {pr.Topping}");
				}
			}
			else
			{
				FileStream fileStream;
				StreamWriter writer;
				TextWriter oldOut = Console.Out;
				try
				{
					Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

					fileStream = new FileStream(outputFilePath, FileMode.OpenOrCreate, FileAccess.Write);
					writer = new StreamWriter(fileStream);
					
					Console.SetOut(writer);

					Console.WriteLine(string.Format("{0} |\t {1} |\t {2} |\t {3} |\t {4} |", "Id".PadRight(30), "Type".PadRight(30), "Name".PadRight(30), "Batter".PadRight(30), "Topping".PadRight(30)));
					Console.WriteLine($"{new string('-', 176)}|");

					foreach (PrintRow pr in query)
					{
						Console.WriteLine($"{pr.Id.PadRight(30)} |\t {pr.Type.PadRight(30)} |\t {pr.Name.PadRight(30)} |\t {pr.Batter.PadRight(30)} |\t {pr.Topping.PadRight(30)} |");
					}

					Console.WriteLine($"{new string('-', 176)}|");

					Console.SetOut(oldOut);
					writer.Close();
					fileStream.Close();

					Console.WriteLine($"Done writing file {outputFilePath}");
				}
				catch (Exception e)
				{
					Console.WriteLine($"Cannot open {outputFilePath} for writing");
					Console.WriteLine(e.Message);
					return;
				}
			}
		}
		class Item
		{
			public string Id { get; set; }
			public string Type { get; set; }
			public string Name { get; set; }
		}

		class Batter
		{
			public string Type { get; set; }
			public Item Parent { get; set; }
		}

		class Toppings
		{
			public string Type { get; set; }
			public Item Parent { get; set; }
		}

		class PrintRow
		{
			public string Id { get; set; }
			public string Type { get; set; }
			public string Name { get; set; }
			public string Batter { get; set; }
			public string Topping { get; set; }
		}
	}
}

