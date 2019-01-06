# 3rd-party Dependencies

The **mc-mapper** project depends on the following 3rd-party software, as licensed:

## LevelDB .NET
*[Original project](https://github.com/oodrive/leveldb.net) &bullet; [Original license grant](https://raw.githubusercontent.com/oodrive/leveldb.net/36c5a3321d7868b8b3b60b1e418d64b00878025b/README.md) - Apache 2.0 License*

LevelDB .NET is used within the [MCBEWorld project](./src/MCBEWorld/LevelDB) in a modified-source-code form.  The original code has been embedded, mostly without modification, to allow access to MCPE world databases.  The only substantial modification has been to enable Zlib compression.

Author's note: I would be open to using LevelDB .NET as a NuGet package.  However, in experimental programs, I encountered problems with compatibility, and of course the officially published package of LevelDB doesn't support Zlib (the level required for MCPE).  I considered publishing my own fork, but I don't want to confuse the issue for any developers using LevelDB.
