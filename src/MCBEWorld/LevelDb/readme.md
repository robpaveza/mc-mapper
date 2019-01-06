# leveldb-MCPE for Windows and .NET #

----------

[leveldb](http://code.google.com/p/leveldb/) is a fast key-value storage library written at Google that provides an ordered mapping from string keys to string values.

This is a fork of the [LevelDB .NET Standard](https://github.com/oodrive/leveldb.net) project specifically to handle
the Mojang [LevelDB-MCPE](https://github.com/mojang/leveldb-mcpe) fork, which provides Zlib compression instead of Snappy.
It does not intend to support generalized LevelDB support.

Whereas the original implementation (oodrive's) included the native .vcxproj (the Visual C++ project), this project continues to do so, but is not currently built to manage that appropriately.  Put another way, this is a work-in-progress; the current fork adds the correct compression type (`CompressionType.Zlib`), but you must include your native binaries alongside your .NET Standard application.
