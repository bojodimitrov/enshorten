# Enshorten

### Getting started

Enshorten is small URL shortening service written in .NET Core 6. Current version uses SQL as storage. In order to run the service you will need .NET SDK for .NET 6.0, which can be downloaded from here: https://dotnet.microsoft.com/en-us/download. Navigate to the project folder (where Enshorten.csproj can be found) and run following commands:

- **dotnet build**
- **dotnet run**

The current version of the project is set to run on URL http://localhost:5000, but if you need it to run on another port, just add the urls parameter to the **dotnet run** command:

- **dotnet run --urls "http://localhost:<port>"**

SQL server is also required for the project. The database is created by running the **create_database.sql** script contained in the project folder. The current version of the app is set up to connect to SQL server on localhost. If you need to change the connection string, you can do it in **appsettings.json** file, where a section **"Connection strings"** could be found. In the "Enshorten" connection string you need to change **"Server=localhost"** to wherever your SQL server is.

### Features

- Shortens a URL and returns a string of length maximum 5 symbols, containing only latin characters and numerals
- Stores the long URL against that string and redirects to it whenever the user navigate to <ENSHORTEN_DOMAIN>/<SHORT_URL>

### Technical solution

Enshorten maps the long URL to the short string by hashing the given long URL. The chosen algorithm for hashing is CRC-32 (https://en.wikipedia.org/wiki/Cyclic_redundancy_check#CRC-32_algorithm) because of its general good distribution of its hashes for short strings, which is our case. However, the algorithm in Enshorten is enhanced for its needs, because the number of all latin symbols and numerals is 62, which for a string with length 5 is **62^5 = 916,132,832** combinations. CRC-32 produces 32 bit string, which means **4,294,967,296** combinations. In order to keep the uniform distribution of the hash function, the following algorithm is used:

- If the produced by CRC-32 number is smaller than **916,132,832**, then it is directly used for the hash
- Otherwise, from the produced number **916,132,832** is substracted; then it is divided by the necessary proportion in order to be brought down under **916,132,832**

Since both translation and scaling are linear transformations, the uniformity of the distribution is kept, albeit in smaller range (thus increasing the probability for hash collisions). Then that number is stored in the database against the long URL. For the conversion from number to 5 symbols long string, a conversion from Decimal to Base 62 is made and vice versa.

#### Hash collisions

With the increase of the data stored in the database, the probability for hash collision is also increased. In order to combat this, open addressing approach is used for finding new available spot. Whenever the same hash is found, and the URL is not the same, probing is done in search for next available larger or smaller number. The probing for smaller and larger numbers is alternated, for example we first try if n+1 number is free, then for n-1, then for n+2, then for n-2, etc. Quadratic probing is used up until 8196, after which it is replaced by linear probing with that size, since pulling up a lot of records from the database could quickly blow up. This approach is effective up until around 0.8 of the hash table is filled up, afterwards the performance degrades very quickly (https://en.wikipedia.org/wiki/Hash_table#/media/File:Hash_table_average_insertion_time.png) This is a problem for this solution, so whenever that amount of load is near, an extension of the current set of requirements must be done. One approach is to add new symbols to the set of allowed symbols- 10 more symbols double the available space; or allowing for strings of length 6, which will free up 62 times more space.
