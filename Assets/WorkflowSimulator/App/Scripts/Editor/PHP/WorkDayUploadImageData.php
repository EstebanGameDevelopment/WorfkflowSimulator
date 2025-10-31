<?php
	
	include 'ConfigurationWorkflowSimulator.php';

	$user_id = $_POST["user"];
	$passw_user = $_POST["password"];
	$salt_user = $_POST["salt"];

	if (CheckValidUser($user_id, $passw_user, $salt_user))
	{
		$id = $_POST["id"];
		$project = $_POST["project"];
		$name = $_POST["name"];
		$size = $_POST["size"];
		
		$data_temporal = $_FILES["data"];
		$data_img = file_get_contents($data_temporal['tmp_name']);	

		UploadImageData($user_id, $id, $project, $name, $size, $data_img);
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
 	 //  ExistsImageIndex
     //-------------------------------------------------------------
	 function ExistsImageIndex($user_par, $id_par)
     {
		// Performing SQL Consult
		$query_story = "SELECT * FROM projectsimages WHERE id = $id_par AND user = $user_par";
		$result_story = mysqli_query($GLOBALS['LINK_DATABASE'],$query_story) or die("Query Error::ExistsImageIndex = $id_par");
		
		if ($row_story = mysqli_fetch_object($result_story))
		{
			return true;
		}
		else
		{
			return false;			
		}
	 }
	 
     //-------------------------------------------------------------
     //  UploadImageData
     //-------------------------------------------------------------
     function UploadImageData($user_par, $id_par, $project_par, $name_par, $size_par, $data_image_par)
     {
		 $id_output = $id_par;
		 
		 $data_par = $data_image_par;
		 
		 if (ExistsImageIndex($user_par, $id_par) == false)
		 {
			// New Data ID
			$query_maxdata = "SELECT max(id) as maximumId FROM projectsimages";
			$result_maxdata = mysqli_query($GLOBALS['LINK_DATABASE'],$query_maxdata) or die("Query Error::UploadImageData::Select max projectsimages failed");
			$row_maxdata = mysqli_fetch_object($result_maxdata);
			$dataid_output =  $row_maxdata->maximumId;
			if ($dataid_output == null) $dataid_output = 0;
			$dataid_output = $dataid_output + 1;
			$id_output = $dataid_output;
			mysqli_free_result($result_maxdata);

			$query_insert = "INSERT INTO projectsimages VALUES (?, ?, ?, ?, ?, ?)";
			$query_insert_speech = mysqli_prepare($GLOBALS['LINK_DATABASE'], $query_insert);
			mysqli_stmt_bind_param($query_insert_speech, 'iiisis', $dataid_output, $user_par, $project_par, $name_par, $size_par, $data_par);
			if (!mysqli_stmt_execute($query_insert_speech))
			{
				die("Query Error::UploadImageData::Insert bookindex Failed::$dataid_output, $user_par, $project_par, $name_par, $size_par, $data_par");
			}
		 }
		 else
		 {
			$query_string = "UPDATE projectsimages SET data = ?, project = ?, name = ?, size = ? WHERE id = ? AND user = ?";
			$query_update_image = mysqli_prepare($GLOBALS['LINK_DATABASE'], $query_string);
			mysqli_stmt_bind_param($query_update_image, 'sisiii', $data_par, $project_par, $name_par, $size_par, $id_par, $user_par);
			if (!mysqli_stmt_execute($query_update_image))
			{
				die("Query Error::UploadImageData::Update projectsimages Failed");
			}
		 }
		 
		 print "true" . $GLOBALS['PARAM_SEPARATOR'] . $id_output;
    }

?>
