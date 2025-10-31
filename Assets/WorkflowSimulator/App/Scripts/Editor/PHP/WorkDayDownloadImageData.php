<?php
	
	include 'ConfigurationWorkflowSimulator.php';
 
	$id = $_GET["id"];
	$direct = $_GET["direct"];
	$user = $_GET["user"];

	DownloadImageData($user, $id, $direct == 1);

    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);
          
     //**************************************************************************************
     //**************************************************************************************
     //**************************************************************************************
     // FUNCTIONS
     //**************************************************************************************
     //**************************************************************************************
     //**************************************************************************************     
	
	 //-------------------------------------------------------------
     //  DownloadImageData
     //-------------------------------------------------------------
     function DownloadImageData($user_par, $id_par, $direct_par)
     {
		$query_consult = "SELECT * FROM projectsimages WHERE id = $id_par AND user = $user_par";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'], $query_consult) or die("Query Error::DownloadImageData::Download image $id_par failed");

		if ($row_data = mysqli_fetch_object($result_consult))
		{
			$name = $row_data->name . ".png";
			$size = $row_data->size;
			$data = $row_data->data;

			if ($direct_par)
			{
				// Set headers to force download
				header('Content-Description: File Transfer');
				header('Content-Type: application/octet-stream');
				header('Content-Disposition: attachment; filename="' . basename($name) . '"');
				header('Content-Length: ' . $size);
				header('Cache-Control: must-revalidate');
				header('Pragma: public');
				header('Expires: 0');

				// Print the binary data
				echo $data;
				exit;				
			}
			else
			{
				print "true" . $GLOBALS['PARAM_SEPARATOR'] . $name . $GLOBALS['PARAM_SEPARATOR'] . $size . $GLOBALS['PARAM_SEPARATOR'] . $data;
			}
		}
		else
		{
			print "false";
		}
		
		mysqli_free_result($result_consult);
    }	
	
?>
