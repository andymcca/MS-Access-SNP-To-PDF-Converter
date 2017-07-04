MS-Access-SNP-To-PDF-Converter
==============================

A tool to convert Microsoft Snapshot files to PDF

Build Requirements 

Visual Studio 2010

.Net Framework 3.5

andymcca Fork
=============

- Added code to remove security from generated PDF files
- Added code to repair broken DEVMODE headers prior to conversion.  This aims to fix the 'Landscape/Half Page' problem that occurs when converting certain Snapshot files.
