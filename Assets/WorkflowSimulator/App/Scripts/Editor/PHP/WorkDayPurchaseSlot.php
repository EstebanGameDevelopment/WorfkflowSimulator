<?php
	
	include 'ConfigurationWorkflowSimulator.php';

	$user_id = $_POST["user"];
	$passw_user = $_POST["password"];
	$salt_user = $_POST["salt"];

	if (CheckValidUser($user_id, $passw_user, $salt_user))
	{
		$slot = $_POST["slot"];
		$level = $_POST["level"];
		$timeout = $_POST["timeout"];
		$receipt_data = $_POST["data"];
		
		if ($slot == -1)
		{
			// INSERT NEW SLOT
			if (RegisterProjectSlot($user_id, $level, $timeout, $receipt_data))
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
			// UPGRADE SLOT
			if (UpgradeProjectSlot($slot, $user_id, $level, $timeout, $receipt_data))
			{
				print "true";
			}
			else
			{
				print "false";
			}
		}			
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
     //  UpgradeProjectSlot
     //-------------------------------------------------------------
     function UpgradeProjectSlot($id_par, $user_par, $level_par, $timeout_par, $receipt_par)
     {
		$query_string = "UPDATE projectslots SET level = ?, timeout = ?, data = ? WHERE id = ? AND user = ?";
		$query_update_data = mysqli_prepare($GLOBALS['LINK_DATABASE'], $query_string);
		mysqli_stmt_bind_param($query_update_data, 'iisii', $level_par, $timeout_par, $receipt_par, $id_par, $user_par);
		if (!mysqli_stmt_execute($query_update_data))
		{
			die("Query Error::UpgradeBookSlot::Purchase projectslots Failed");
			return false;
		}
		else
		{
			return true;
		}
	 }

?>
