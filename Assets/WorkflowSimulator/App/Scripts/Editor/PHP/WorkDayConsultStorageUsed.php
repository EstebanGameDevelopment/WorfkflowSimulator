<?php
	
	include 'ConfigurationWorkflowSimulator.php';
 
	$id_user = $_GET["id"];
	$passw_user = $_GET["password"];
	$salt_user = $_GET["salt"];
	$project = $_GET["project"];

	if (CheckValidUser($id_user, $passw_user, $salt_user))
	{
		$data_storage = ConsultStorageData($id_user, $project);
		$images_storage = ConsultStorageImages($id_user, $project);		
		
		print "true" . $GLOBALS['PARAM_SEPARATOR'] . $data_storage . $GLOBALS['PARAM_SEPARATOR'] . $images_storage;
	}
	else
	{
		print "false";
	}		

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
     //  ConsultStorageData
     //-------------------------------------------------------------
     function ConsultStorageData($user_par, $project_par)
     {
		$query_consult = "SELECT OCTET_LENGTH(data) AS data_size FROM projectsdata WHERE id = $project_par AND user = $user_par";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'], $query_consult) or die("Query Error::ConsultStorageData::Select for project $project_par failed");

		$total_size = 0;
		if ($row_image = mysqli_fetch_object($result_consult))
		{
			$total_size = $row_image->data_size;
		}
		
		mysqli_free_result($result_consult);
		
		return $total_size;
    }	
	
	 //-------------------------------------------------------------
     //  ConsultStorageImages
     //-------------------------------------------------------------
     function ConsultStorageImages($user_par, $project_par)
     {
		$query_consult = "SELECT SUM(size) AS total_size FROM projectsimages WHERE project = $project_par AND user = $user_par";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'], $query_consult) or die("Query Error::ConsultStorageImages::Select for project $project_par failed");

		$total_size = 0;
		if ($row_image = mysqli_fetch_object($result_consult))
		{
			$total_size = $row_image->total_size ?? 0;
		}
		
		mysqli_free_result($result_consult);
		
		return $total_size;
    }	

	
?>
