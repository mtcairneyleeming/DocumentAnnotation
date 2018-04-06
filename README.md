# Introduction 
A system for annotating text documents (basically, my Latin set texts). It uses ASP.NET Core 2 Razor Pages to serve a web UI, jQuery on the front end to make it all work, PostgreSQL as storage for annotations and users, and a python CLI program for doing stuff to xml documents.
The python CLI takes a TEI.2 xml document, as all the texts on [Perseus](http://www.perseus.tufts.edu) are downloadable, and recursively goes through it to pick out the document's actual text and outputs a massively simplified, **standard** version. This transformation has to be run on any text before the C# server can serve the actual text.

# Getting Started
To start, clone this repo, then follow the instructions below.
## 1. Python transformer
This is made up of two files in `server/TextLoader/python`, and should be run in Python 3. Use this transformer, run as a CLI by `cli.py`, to transform texts of your choice (e.g. the Aeneid, or Cicero's Orationes) so that the C# server can use them. 
### Usage
```
usage: cli.py [-h] --input-directory INPUT --output-directory OUTPUT
              [--identifier IDENTIFIER] [--add-new-line NEWLINE]

optional arguments:
  -h, --help                              show this help message and exit
  --input-directory INPUT, -i INPUT       the directory where the original XML files are stored
  --output-directory OUTPUT, -o OUTPUT    the directory where processed .json.gz files will be stored
  --identifier IDENTIFIER, -f IDENTIFIER  the identifier, in the form YEAR.MONTH.NUM, 
                                          e.g. 1999.02.0055, for the text you want to add
  --add-new-line NEWLINE, -n NEWLINE      whether to add a new line after each groups of words 
                                          (True for verse, as a group is a line, 
                                          but False for prose, as groups are (roughly) sentences)
```
### Examples
Running on a single text, Cicero's _Orationes_, which does not have line breaks between groups
```
py -3 .\python\cli.py -i "~\DocumentAnnotation\server\TextLoader\OriginalTexts" -o "~\DocumentAnnotation\server\TextLoader\processed" -f "1999.02.0011" -n False
```
Running on a single text, Virgil's _Aeneid_, which does have line breaks between groups (as it's groups are lines)
```
py -3 .\python\cli.py -i "~\DocumentAnnotation\server\TextLoader\OriginalTexts" -o "~\DocumentAnnotation\server\TextLoader\processed" -f "1999.02.0055" -n True
```
Running on a folder of texts. (**NB:** currently there is no support for working out whether or not to add line breaks, so running it on a folder of prose and verse will get half of it wrong)
```
py -3 .\python\cli.py -i "~\DocumentAnnotation\server\TextLoader\OriginalTexts" -o "~\DocumentAnnotation\server\TextLoader\processed"  -n True
```

## 2. Database
Install PostgreSQL, and create a database and secure it however you wish. (The default is an unsecured database named annotations, on the local server, on the default port.) Update the connection string in the `appsettings[.Development].json` files to connect to your database.
Having installed dependencies (see above), run `dotnet ef database update` so that the required tables are added, though no data will be present.

## 3. C# server
To install dependencies, run `dotnet restore`. This will download all the libraries and tools needed, including the DB management tools.
If you have moved where processed texts are stored, update the `appsettings[.Development].json` files to reflect this.
To run the server, `dotnet build` and `dotnet run` will build and serve the UI on the preconfigured port (by default 5000 - a future addition will be the ability to change this). See [the .NET Core 2 CLI docs](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x) for more details.

(All these commands should be run in the `server` directory)

## 4. Initial set up
For the moment, data about the texts you have processed is not automatically added to the database, so on first startup, after registering as a new user, you must create TextData entries for each text you processed, by navigating to `/Texts/Create`. For example, for Cicero's Orationes, you should create an entry with the following attributes:
Title: Orationes
Author: M. Tullius Cicero
Abbreviation: Cic. (for Perseus links)
Identifier: 1999.02.0011

After this, your processed texts will be available to read and annotate.

# Usage
To annotate a text, once the Getting Started section has been followed, create a new Document (at `/Documents/Create`). Once you have done this, both the `/Documents` page and the home page will show your new Document, and you can annotate it.

# Authentication and Users
At current, registration is available to the public, if they can connect to the server. Future enhancements will include adding an Admin role, and create, update and delete actions on texts will be available to only Admins.
