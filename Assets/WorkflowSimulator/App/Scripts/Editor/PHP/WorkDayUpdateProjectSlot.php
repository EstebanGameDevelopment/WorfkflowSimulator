<?php
	
	include 'ConfigurationWorkflowSimulator.php';

	$user_id = $_POST["user"];
	$passw_user = $_POST["password"];
	$salt_user = $_POST["salt"];

	if (CheckValidUser($user_id, $passw_user, $salt_user))
	{
		$slot = $_POST["slot"];
		$project = $_POST["project"];
		
		// UPGRADE SLOT
		if (UpdateProjectSlot($user_id, $slot, $project))
		{
			print "true";
		}
		else
		{
			print "false";
		}
	}
	else
	{
		print "false";
	}	

    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);

?>
