<?php
	
	include 'ConfigurationWorkflowSimulator.php';

	$user_id = $_POST["user"];
	$passw_user = $_POST["password"];
	$salt_user = $_POST["salt"];

	if (CheckValidUser($user_id, $passw_user, $salt_user))
	{
		$id = $_POST["id"];
		
		$data_temporal = $_FILES["data"];
		$data = file_get_contents($data_temporal['tmp_name']);	
		
		// ++ LOGIN WITH email ++
		UploadData($user_id, $id, $data, true);
	}
	else
	{
		print "false";
	}	

    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);

?>
