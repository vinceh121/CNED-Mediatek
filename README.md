# CNED-Mediatek

This repo hosts an exercise project, part of the CNED's BTS SIO SLAM course. The project is a desktop app to be written in C# and use MySQL/MariaDB as DBMS.

It's a simple desktop app with direct acces to the DB. It is comprised of 2 CRUDs:
 - Employee management
 - Employee leave management

Being an exclusively Debian GNU/Linux user, setting up a C# development environment was an initial unintended challenge.

A few components that were likely expected (although not explicitly required) to be used were either far from ideal, or impossible to use:

 - WPF as UI framework
 - MySql.Data as DBMS connector
 - Visual Studio's Installer Project extension as installer framework
 - SandCastle as API doc generator

Of course if you were working with Windows those choices seem fully benign, however WPF, Visual Studio and SandCastle are not available at all on *Nix systems.
That is going past Mono's crude WPF implementation that remains unusable for a development environment.
SandCastle has been abandoned since 2012.

Therefore, the following components were used instead:
 - GTKSharp as a UI framework
 - MySqlConnector as DBMS connector
 - Nullsoft Scriptable Install System as installer framework
 - Doxygen as API doc generator

GTK# is fully cross-platform, both as shared and static linking.
MySql.Data lacks asynchronous methods, and lacks compatibility with other MySql forks compared to MySqlConnector.
NSIS is cross-platform and commonly used.
Doxygen is fully crossplatform and commonly used.

## License

Copyright (C) 2023 Vincent Hyvert

This program is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Affero General Public License for more details.

Read the LICENSE file to consult the full GNU Affereo General Public License.
