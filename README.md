# EvilExtractor

In short, it's a GUI application I made for running the "Prey (2017) pak decryption tool" on multiple files, sequentially. 

## How to use

### Decrypting the Prey *.pak files

* Get a copy of Prey installed
* Get the [pak decryption tool](https://forum.xentax.com/viewtopic.php?f=10&t=16241#p130356) from the xentax forums
* Build or download a realease of this, and run it!
* Set the Input File Extension to "*.pak"
* Set the Output File Extension to "*.zip"
* Select the Extraction Tool file (the pak decryption tool you downloaded from the xentax thread)
* Select the Input Folder - for example: "D:\Steam\steamapps\common\Prey\GameSDK" if you want to decrypt ALL the files (otherwise copy the files you want to a new folder and select that)
* Select the Output Folder
* Click Start, you will get a confirmation for how many files are going to be extracted
* Wait for the tool to do its things

### Decrypting the Prey *.xml files

* Extract the *.xml files from the decrypted *.zip files after using the above method
* Get the [xml decrpytion tool](https://forum.xentax.com/viewtopic.php?f=10&t=16241#p130429) from the xentax forums
* Run the EvilExtractor
* Set the Input File Extension to "*.xml"
* Set the Output File Extension to "*.xml"
* Select the Extraction Tool file (the xml decryption tool you downloaded from the xentax thread)
* Select the Input Folder - the one where you unzipped the decrypted pak files (or copy the xml-s that interest you to a separate folder and select that)
* Select the Output Folder
* Click Start, you will get a confirmation for how many files are going to be extracted
* Wait for the tool to do its things

### Other uses?
The application basically runs a process of an executable that takes two arguments: an input file, and an output file. For example, something like this:
```
tool.exe "D:\DataIn\InputFile.pak" "D:\DataOut\OutputFile.zip"
```

You must specify an extension for your input files, and an extension for your output files. Only files with the given input extension will be used when running the executable tool, and they will be given the output extension.

You must set the path to the executable that will perform the extraction, and also set an input and output folders.

The application will run the specified executable as a process, sequentially, on all the files that have the same input extension in the input folder.

As long as your executable uses the same argument pattern, you can use this to run it on multiple files.

## Releases

* [EvilExtractor V1.0](/evilvasile/EvilExtractor/releases/download/v1.0/EvilExtractor.exe)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE) file for details
