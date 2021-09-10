# JsonParser
This .NET Core console application is written with C#.

Basically, the Newtonsoft.json library is used to read the JSON content and LINQ is used to reorganized the data into object lists. To achieve data normalised form, the easiest and most efficient way is to utilize the table outer join approach. The LINQ does have the functionality to do outer join between lists. This solution basically is developed based on these 2 concepts.

Usage: .\JsonParser.exe "sample.json" ".\test\test.table"

By supplying a JSON that consist of Item, Batter and Topping:
![image](https://user-images.githubusercontent.com/90307837/132856197-ba4a764b-a9f8-44ed-943b-933168f74f26.png)

The example output:
![image](https://user-images.githubusercontent.com/90307837/132855944-6a8025c0-d0b1-4030-a879-1c1b7629c773.png)
