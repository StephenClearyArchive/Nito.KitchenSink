@echo off
REM This file requires Debugging Tools for Windows, Perl (e.g., ActivePerl), and svn.exe (e.g., CollabNet/Tigris)
call "c:\Program Files\Debugging Tools for Windows (x86)\srcsrv\ssindex.cmd" /SYSTEM=SVN /SYMBOLS=Nito.KitchenSink\bin\Release /Debug
pause