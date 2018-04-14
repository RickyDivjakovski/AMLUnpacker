# AMLUnpacker
## Information
An opensource executable and class library to split and unpack aml_upgrade_package.img files.
(Yes the code is extremely messy, but it does work)

## Usage
Executable -
	AMLUnpackerExecutable.exe -u aml_upgrade_package.img MyOutputFolder
	
Class library -
	Refference AMLUnpacker.dll
	AMLUnpacker unpacker = new AMLUnpacker();
	unpacker.Unpack("aml_upgrade_package.img", "MyOutputFolder");

## Working
-Splitting aml_upgrade_package.img files.
-Generating partition info text file.

## TO-DO
-Add unpacking system.PARTITION
-Add unpacking boot.PARTITION
-Add unpacking recovery.PARTITION
-Few other things

## Bugs
-Null

## Contributors
Ricky Divjakovski
perpetual@freaktab.com