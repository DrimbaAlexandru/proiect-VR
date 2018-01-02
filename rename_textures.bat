@ECHO OFF
SETLOCAL EnableDelayedExpansion
set file_counter=1

FOR /F %%f IN (' DIR /B^|grep .*\.texture\.txt') DO (

	set filename=%%f
	SET primary=undefined
	FOR /F "tokens=* USEBACKQ" %%F IN (`grep -E -o "Primary *= *(([a-zA-Z0-9\-\_ ]*)\.[a-zA-Z]+)" !filename!`) DO (
		SET primary=%%F
	)
	if "%primary%" == "undefined" (
		Echo Matching failed!
	) else (

		FOR /F "tokens=* USEBACKQ" %%F IN (`ECHO !primary!^| sed -r "s/([a-zA-Z]+) *= *//g"`) DO (
			SET texture_file=%%F
		)
		FOR /F "tokens=* USEBACKQ" %%F IN (`ECHO !texture_file!^| sed -r "s/(\.[a-zA-Z]+)//g"`) DO (
			SET file_name=%%F
		)
		FOR /F "tokens=* USEBACKQ" %%F IN (`ECHO !texture_file!^| sed -r "s/([a-zA-Z0-9\-\_ ]+)//"`) DO (
			SET extension=%%F
		)
		 
		sed -i "s/!file_name!/!file_counter!/g" !filename!
		
		unix2dos !filename!
		
		set new_name=!file_counter!!extension!
			
		ren !texture_file! !new_name!
		set /a "file_counter+=1"
		
	)
)