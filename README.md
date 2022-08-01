# TableToImageExport
**A C# library for processing tabular data into images.**

## Description
This is a new cross-platform library capable of processing tabular data into images. This library provides full customizability options for tables and has support for CSV/TSV file formats.

Additionally, tables can be completely modified or created manually from scratch. This library supports a variety of content types such as images and text with more planned types in future updates.

You can see some examples below, these are the raw image files which have been produced from this library:
### Example 1:
![molecules](https://user-images.githubusercontent.com/87785573/181994622-a46bdde5-429a-4332-9de2-c75ed214616c.png)

Produced from this CSV file:
```
Compound, Structure, Formula, Density, Freezing Point (°C), Boiling Point (°C)
Oxygen (Dioxygen), ., O₂, 1.292kg/cm³, -218°C, -183°C
Hydrogen (Dihydrogen), ., H₂, 0.08375kg/cm³, -259.2°C, -252.9°C
Water, ., H₂O, 1g/cm³, 0°C, 100°C
Carbon Dioxide, ., CO₂, 1.87kg/cm³, -78°C, -57°C
Carbon Monoxide, ., CO, 1.14kg/cm³, -205°C, -191.5°C
Ammonia, ., NH₃, 0.73kg/cm³, -77.3°C, -33.4°C
Methane, ., CH₄, 0.657kg/cm³, -182°C, -161.6°C
Salt (Sodium Chloride), ., NaCl, 2.16 g/cm³, 801°C, 1465°C
Hydrogen Peroxide, ., H₂O₂, 1.45 g/cm³, -0.4°C, 150°C
```
###### *Since the file itself cannot store images, a placeholder column has been added into the file which is replaced in code.*

### Example 2:
![employees](https://user-images.githubusercontent.com/87785573/182039577-88dc8cc5-a03a-4fcf-b052-7ef4f3c1fa84.png)

Produced from this list of records:
```C#
List<Employee> employees = new List<Employee>()
{
  new Employee("C3056", "David Navarro", "393 Greenford Road", "GU8 5QT", "0161 496 0141", new DateTime(2009, 12, 2)),
  new Employee("C3203", "Anne Tucker", "313 Bedfont Lane", "RH14 0AT", "0161 496 0664", new DateTime(2011, 3, 28)),
  new Employee("C3413", "Alexis Martin", "24 Chapel House Drive", "B21 0SU", "0161 496 0914", new DateTime(2012, 10, 28)),
  new Employee("C3047", "Keith Levy", "25 Knostrop Quay", "SL9 7QW", "0161 496 0570", new DateTime(2009, 12, 29)),
  new Employee("C3959", "Donna Gomez", "16 Chester Road", "NG34 9TU", "0161 496 0400", new DateTime(2009, 9, 20)),
  new Employee("C3320", "Barry Smith", "17 Springcroft Drive", "DN9 1SF", "0161 496 0787", new DateTime(2020, 12, 10)),
  new Employee("C3104", "Erik Jackson", "52 Groby Lane", "BS8 3ST", "0161 496 0598", new DateTime(2006, 10, 1)),
  new Employee("C3684", "Mark Hill", "24 King Street", "LL77 7RD", "0161 496 0747", new DateTime(2008, 12, 19)),
  new Employee("C3293", "Marissa Gomez", "76 Market Place", "GL50 2NG", "0161 496 0569", new DateTime(2014, 11, 9)),
  new Employee("C3918", "Jonathan Griffin", "117 Worthing Road", "NG21 0TA", "0161 496 0313", new DateTime(2015, 6, 3)),
  new Employee("C3183", "Bradley Mcgee", "7 Woodchester Road", "RH5 6SF", "0161 496 0252", new DateTime(2008, 1, 12)),
  new Employee("C3019", "Michelle Novak", "63 Nineveh Road", "CA18 1RR", "0161 496 0695", new DateTime(2017, 1, 30)),
  new Employee("C3193", "Joseph Wilson", "33 Lansdown Crescent", "WF8 3EL", "0161 496 0859", new DateTime(2006, 10, 4)),
  new Employee("C3492", "Toni Merritt", "8 Wright Close", "LS10 1GG", "0161 496 0587", new DateTime(2022, 5, 8))
};
```
Using this record:
```C#
public record Employee(string EmployeeID, string FullName, string Address, string Postcode, string PhoneNumber, DateTime DateEmployed);
```


## Installation (For Visual Studio 2022)
This library is available as a nuget package under the same name TableToImageExport.

This library is not compatible with Visual Studio 2019 since this library is .NET 6.0

### Install through Nuget Command Line:
1. Open a new package manager console. (Tools > NuGet Package Manager > Package Manager Console).
2. Find the new console and enter `Install-Package TableToImageExport`.
3. Done!

### Install through Nuget Package Manager:
1. Right click your project where you want to use this library in the solution explorer.
2. Select "Manage Nuget Packages...".
3. Click "Browse" tab and select the search box.
4. Type the name "TableToImageExport", you can also find this library by various keywords (E.g. Images Tables).
5. Find this package and click install.
6. Since this library has dependencies on other libraries (CSVHelper), you will need to accept the license for those other libraries.
7. Done!

## Documentation/Guide:
See the [wiki](https://github.com/liammct4/TableToImageExport/wiki).

## Planned Changes:
- Support for HTML generation.
- Cell spanning over columns and rows.
- Sub cell support.

## Acknowledgements/Dependencies
Below is the list of all the other libraries needed for this library to work:
- [CSVHelper](https://joshclose.github.io/CsvHelper/)
- [SixLabors.ImageSharp](https://sixlabors.com/products/imagesharp/)
- [SixLabors.ImageSharp.Drawing](https://github.com/SixLabors/ImageSharp.Drawing)
