<?php

	use mail\PHPMailer;
	use mail\Exception;	
	
	header('Content-type: text/html; charset=utf-8');
    
	// Connecting, selecting database
	$LINK_DATABASE = mysqli_connect("localhost", "root", "")
       or die("Could not connect");
    // print "Connected successfully<p>";
    mysqli_select_db($LINK_DATABASE, "workflowsimulator") or die("Database Error::Could not select database)");

	// RESPONSES WILL INCLUDE SPECIAL CHARACTERS BECAUSE THEY CONTAIN USERS' WORDS
	mysqli_query ($LINK_DATABASE, "set character_set_client='utf8'"); 
	mysqli_query ($LINK_DATABASE, "set character_set_results='utf8'"); 
	mysqli_query ($LINK_DATABASE, "set collation_connection='utf8_general_ci'");

	// OFFICIAL NAME OF THE APPLICATION
	$OFFICIAL_NAME_APPLICATION_GLOBAL = " Workflow Simulator ";

	// THE DEFAULT PROJECT THAT WILL BE COPIED WHEN THE USER REGISTERS
	$ID_TEMPLATE_DEFAULT_PROJECT = 2;
	$NAME_TEMPLATE_DEFAULT_PROJECT = "Workflow Simulator";
	$DESCRIPTION_TEMPLATE_DEFAULT_PROJECT = "Workflow Simulator";
	
	// ADDRESS OF YOUR SERVER
	$URL_BASE_SERVER = "http://localhost:8080/workflowsimulator/";
	
	// WILL ENABLE THE EMAIL SYSTEM FOR ACCOUNT CONFIRMATION AND OTHER OPERATIONS
	$ENABLE_EMAIL_SERVER = 1;
	
	// SEPARATOR TOKENS USED IN HTTPS COMMUNICATIONS
	$BLOCK_SEPARATOR = "<block>";
	$PARAM_SEPARATOR = "<par>";
	$LINE_SEPARATOR = "<line>";
	$USER_DATA_SEPARATOR = "<udata>";

	// EMAIL ADDRESSES
	$NON_REPLY_EMAIL_ADDRESS = 'no-reply@workflowsimulator.com';
	$ANALYSIS_EMAIL_ADDRESS = 'info@workflowsimulator.com';

	// REPORT ABOUT THE SERVICE BEING ACTIVE
	$SERVER_SERVICE_ACTIVATED = 'true';
	
	 //-------------------------------------------------------------
     //  SpecialCharacters
     //-------------------------------------------------------------
     function SpecialCharacters($text_plain_par)
     {
		$output_text = $text_plain_par;
		$output_text = str_replace('"', '\"', $output_text);
		$output_text = str_replace('\'', '`', $output_text);
		$output_text = str_replace('&', '\&', $output_text);
		
		return $output_text;
	 }

	  //-------------------------------------------------------------
     //  GetCurrentTimestamp
     //-------------------------------------------------------------
	function GetCurrentTimestamp()
    {
		$datebeging = new DateTime('1970-01-01');
		$currDate = new DateTime();
		$diff = $datebeging->diff($currDate);
		$secs=$diff->format('%a') * (60*60*24);  //total days
		$secs+=$diff->format('%h') * (60*60);     //hours
		$secs+=$diff->format('%i') * 60;              //minutes
		$secs+=$diff->format('%s');                     //seconds
		return $secs;
     }
	
	 //-------------------------------------------------------------
 	 //  CheckValidUser
     //-------------------------------------------------------------
	 function CheckValidUser($iduser_par, $password_par, $salt_par)
     {
		$password_hashed = HashPasswordWithSalt($password_par, $salt_par);
		$email_found = ExistsHashedUser($iduser_par, $password_hashed);
		return !empty($email_found);
	 }
	 
	 //-------------------------------------------------------------
 	 //  ExistsUser
     //-------------------------------------------------------------
	 function ExistsUser($iduser_par, $password_par)
     {
		// Performing SQL Consult
		$query_user = "SELECT id,email,password,registerdate,lastlogin FROM users WHERE id = $iduser_par";
		$result_user = mysqli_query($GLOBALS['LINK_DATABASE'],$query_user) or die("Query Error::ConfigurationUserManagement::ExistsUser");
		
		if ($row_user = mysqli_fetch_object($result_user))
		{
			if (VerifySaltPassword($password_par, $row_user->password, strval($row_user->registerdate + $row_user->lastlogin)))
			{
				return $row_user->email;
			}
			else
			{
				return "";	
			}
		}
		else
		{
			return "";
		}
	 }

	 //-------------------------------------------------------------
 	 //  ExistsHashedUser
     //-------------------------------------------------------------
	 function ExistsHashedUser($iduser_par, $password_par)
     {
		// Performing SQL Consult
		$query_user = "SELECT id,email,password FROM users WHERE id = $iduser_par";
		$result_user = mysqli_query($GLOBALS['LINK_DATABASE'],$query_user) or die("Query Error::ConfigurationUserManagement::ExistsHashedUser");
		
		if ($row_user = mysqli_fetch_object($result_user))
		{
			if ($row_user->password == $password_par)
			{
				return $row_user->email;
			}
			else
			{
				return "";
			}
		}
		else
		{
			return "";
		}
	 }

	 //-------------------------------------------------------------
     //  HashPassword
     //-------------------------------------------------------------
     function HashPassword($password_par)
     {
		$hashedPassword = password_hash($password_par, PASSWORD_DEFAULT);
		return $hashedPassword;
	 }
  
  	 //-------------------------------------------------------------
     //  HashPasswordWithSalt
     //-------------------------------------------------------------	 
	 function HashPasswordWithSalt($password, $salt) 
	 {
		return base64_encode(hash('sha256', $password . $salt, true));
	 }
	 
	 //-------------------------------------------------------------
     //  VerifyPassword
     //-------------------------------------------------------------
     function VerifyPassword($password_par, $hashedPassword_par)
     {
		return (password_verify($password_par, $hashedPassword_par));
	 }

	 //-------------------------------------------------------------
     //  VerifySaltPassword
     //-------------------------------------------------------------
     function VerifySaltPassword($password_par, $hashedPassword_par, $salt_par)
     {
		return $hashedPassword_par == HashPasswordWithSalt($password_par, $salt_par);
	 }
	 
	 //-------------------------------------------------------------
 	 //  CleanCharactersFromString
     //-------------------------------------------------------------
	 function CleanCharactersFromString($string_par)
     {
		return mysqli_real_escape_string($GLOBALS['LINK_DATABASE'], $string_par);
	 }
	 
	 //-------------------------------------------------------------
     //  rand_string
     //-------------------------------------------------------------
	 function rand_string( $length_par ) 
	 {
		$chars = "abcdefghijklmnopqrstuvwxyz0123456789";
		return substr(str_shuffle($chars),0,$length_par);
	}

	 //-------------------------------------------------------------
     //  IsLittleEndian
     //-------------------------------------------------------------
	function IsLittleEndian() 
	{
		$testint = 0x00FF;
		$p = pack('S', $testint);
		return $testint===current(unpack('v', $p));
	}
	
	 //-------------------------------------------------------------
 	 //  ExistsProjectIndex
     //-------------------------------------------------------------
	 function ExistsProjectIndex($id_par)
     {
		// Performing SQL Consult
		$query_project = "SELECT * FROM projectsindex WHERE id = $id_par";
		$result_project = mysqli_query($GLOBALS['LINK_DATABASE'],$query_project) or die("Query Error::ConfigurationUserManagement::ExistsProjectIndex");
		
		if ($row_story = mysqli_fetch_object($result_project))
		{
			return true;
		}
		else
		{
			return false;			
		}
	 }
	 
	 //-------------------------------------------------------------
 	 //  ExistsProjectData
     //-------------------------------------------------------------
	 function ExistsProjectData($user_par, $id_par)
     {
		// Performing SQL Consult
		$query_data = "SELECT * FROM projectsdata WHERE id = $id_par AND user = $user_par";
		$result_data = mysqli_query($GLOBALS['LINK_DATABASE'],$query_data) or die("Query Error::ConfigurationUserManagement::ExistsProjectData");
		
		if ($row_data = mysqli_fetch_object($result_data))
		{
			return true;
		}
		else
		{
			return false;			
		}
	 }

	 //-------------------------------------------------------------
     //  DeleteProjectData
     //-------------------------------------------------------------
     function DeleteProjectData($user_par, $project_par)
     {
		$query_delete_index = "DELETE FROM projectsindex WHERE data = $project_par AND user = $user_par";
		if ($project_par == -1) $query_delete_index = "DELETE FROM projectsindex WHERE user = $user_par";
		mysqli_query($GLOBALS['LINK_DATABASE'], $query_delete_index) or die("Query Error::DeleteProjectData::Failed to delete projectsindex story $project_par");

		$query_delete_data = "DELETE FROM projectsdata WHERE id = $project_par AND user = $user_par";
		if ($project_par == -1) $query_delete_data = "DELETE FROM projectsdata WHERE user = $user_par";
		mysqli_query($GLOBALS['LINK_DATABASE'], $query_delete_data) or die("Query Error::DeleteProjectData::Failed to delete projectsdata story $project_par");

		return true;
    }

     //-------------------------------------------------------------
     //  DeleteImageData
     //-------------------------------------------------------------
     function DeleteImageData($user_par, $project_par, $id_par)
     {
		$query_delete_image = "DELETE FROM projectsimages WHERE id = $id_par AND user = $user_par AND project = $project_par";
		if (($project_par != -1) && ($id_par == -1)) $query_delete_image = "DELETE FROM projectsimages WHERE user = $user_par AND project = $project_par";
		if (($project_par == -1) && ($id_par != -1)) $query_delete_image = "DELETE FROM projectsimages WHERE id = $id_par AND user = $user_par";
		if (($project_par == -1) && ($id_par == -1)) $query_delete_image = "DELETE FROM projectsimages WHERE user = $user_par";
		mysqli_query($GLOBALS['LINK_DATABASE'], $query_delete_image) or die("Query Error::DeleteImageData::Failed to delete image $id_par");
		
		if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
    }

	 //-------------------------------------------------------------
 	 //  ExistsAdmin
     //-------------------------------------------------------------
	 function ExistsAdmin($iduser_par, $password_par)
     {
		// Performing SQL Consult
		$query_user = "SELECT id,email,password,registerdate,lastlogin FROM users WHERE id = $iduser_par AND admin = 1";
		$result_user = mysqli_query($GLOBALS['LINK_DATABASE'],$query_user) or die("Query Error::ConfigurationUserManagement::ExistsUser");
		
		if ($row_user = mysqli_fetch_object($result_user))
		{
			if (VerifySaltPassword($password_par, $row_user->password, strval($row_user->registerdate + $row_user->lastlogin)))
			{
				return $row_user->email;
			}
			else
			{
				return "";	
			}
		}
		else
		{
			return "";
		}
	 } 

	 //-------------------------------------------------------------
 	 //  ExistsEmail
     //-------------------------------------------------------------
	 function ExistsEmail($emailuser_par)
     {
		// Performing SQL Consult
		$query_user = "SELECT * FROM users WHERE email = '$emailuser_par'";
		$result_user = mysqli_query($GLOBALS['LINK_DATABASE'],$query_user) or die("Query Error::ConfigurationUserManagement::ExistsUser");
		
		if ($row_user = mysqli_fetch_object($result_user))
		{
			return true;
		}
		else
		{
			return false;			
		}
	 }

     //-------------------------------------------------------------
     //  LoginEmail
     //-------------------------------------------------------------
     function LoginEmail($email_par, $password_par)
     {
		// Performing SQL Consult
		$query_user = "SELECT * FROM users WHERE email = '$email_par'";
		$result_user = mysqli_query($GLOBALS['LINK_DATABASE'],$query_user) or die("Query Error::UserLoginByEmail::Select users failed");
				
		if ($row_user = mysqli_fetch_object($result_user))
		{
			$registerdate_user = $row_user->registerdate;
			$lastlogin_user = $row_user->lastlogin;
			if (VerifySaltPassword($password_par, $row_user->password, strval($registerdate_user + $lastlogin_user)))
			{
				$id_user = $row_user->id;
				$name_user = $row_user->nickname;							
				$admin_user = $row_user->admin;
				$level_user = $row_user->level;
				$code_user = $row_user->code;
				$validated_user = $row_user->validated;
				$platform_user = $row_user->platform;

				$current_time_login = GetCurrentTimestamp();
				
				// REHASH THE PASSWORD
				$password_rehashed = HashPasswordWithSalt($password_par, strval($registerdate_user + $current_time_login));
				
				// UPDATE ENERGY
				$query_update_user = "UPDATE users SET lastlogin=$current_time_login, password='$password_rehashed' WHERE id = $id_user";
				$result_update_user = mysqli_query($GLOBALS['LINK_DATABASE'],$query_update_user) or die("Query Error::UserLoginByEmail::Update users failed");

				if ($result_update_user)
				{
					$output_total_login_data = "true" . $GLOBALS['USER_DATA_SEPARATOR'] .  $id_user . $GLOBALS['PARAM_SEPARATOR'] . $email_par . $GLOBALS['PARAM_SEPARATOR'] . $name_user . $GLOBALS['PARAM_SEPARATOR'] . $registerdate_user . $GLOBALS['PARAM_SEPARATOR'] . $current_time_login . $GLOBALS['PARAM_SEPARATOR'] . $admin_user . $GLOBALS['PARAM_SEPARATOR'] . $level_user . $GLOBALS['PARAM_SEPARATOR'] . $validated_user . $GLOBALS['PARAM_SEPARATOR'] . $platform_user;
					$output_total_login_data = $output_total_login_data . GetUserProfileData($id_user);
					
					print $output_total_login_data;				
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
		}
		else
		{
			print "false";
		}
	 
		// Free resultset
		mysqli_free_result($result_user);
    }

	//-------------------------------------------------------------
	//  ConsultUser
	//-------------------------------------------------------------
     function ConsultUser($user_par, $get_facebook_par, $get_profile_par)
     {
		// ++ GET MAX ID ++
		$query_consult = "SELECT * FROM users WHERE id = $user_par";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'],$query_consult) or die("Query Error::UserConsult::Select users failed");

		$package = "";
		if ($row_user = mysqli_fetch_object($result_consult))
		{
			$id_user = $row_user->id;
			$email_user = $row_user->email;
			$password_user = $row_user->password;
			$name_user = $row_user->nickname;
			$registerdate_user = $row_user->registerdate;
			$lastlogin_user = $row_user->lastlogin;
			$admin_user = $row_user->admin;
			$level_user = $row_user->level;
			$code_user = $row_user->code;
			$validated_user = $row_user->validated;
			$platform_user = $row_user->platform;
			
			$output_packet = "true" . $GLOBALS['USER_DATA_SEPARATOR'] .  $id_user . $GLOBALS['PARAM_SEPARATOR'] . $email_user . $GLOBALS['PARAM_SEPARATOR'] . $name_user . $GLOBALS['PARAM_SEPARATOR'] . $registerdate_user . $GLOBALS['PARAM_SEPARATOR'] . $lastlogin_user . $GLOBALS['PARAM_SEPARATOR'] . $admin_user . $GLOBALS['PARAM_SEPARATOR'] . $level_user . $GLOBALS['PARAM_SEPARATOR'] . $validated_user . $GLOBALS['PARAM_SEPARATOR'] . $platform_user;
			if ($get_profile_par) $output_packet = $output_packet . GetUserProfileData($id_user);
			
			print $output_packet;
		}
		else
		{
			print "false";
		}
		
		mysqli_free_result($result_consult);
    }

	 //-------------------------------------------------------------
     //  GetUserProfileData
     //-------------------------------------------------------------
     function GetUserProfileData($iduser_par)
     {
		$query_consult = "SELECT * FROM profile WHERE user = $iduser_par";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'],$query_consult) or die("Query Error::GetUserProfileData::Select PROFILE failed");
		
		if ($row_user = mysqli_fetch_object($result_consult))
		{
			$profile_id = $row_user->id;
			$profile_name = $row_user->name;
			$profile_address = $row_user->address;
			$profile_description = $row_user->description;
			$profile_data = $row_user->data;
			$profile_data2 = $row_user->data2;
			$profile_data3 = $row_user->data3;
			$profile_data4 = $row_user->data4;
			$profile_data5 = $row_user->data5;
			$profile_autorun = $row_user->autorun;
			
			return $GLOBALS['USER_DATA_SEPARATOR'] . 'PROFILE' . $GLOBALS['PARAM_SEPARATOR'] .  $profile_id . $GLOBALS['PARAM_SEPARATOR'] . $profile_name . $GLOBALS['PARAM_SEPARATOR'] . $profile_address . $GLOBALS['PARAM_SEPARATOR'] . $profile_description . $GLOBALS['PARAM_SEPARATOR'] . $profile_data . $GLOBALS['PARAM_SEPARATOR'] . $profile_data2 . $GLOBALS['PARAM_SEPARATOR'] . $profile_data3 . $GLOBALS['PARAM_SEPARATOR'] . $profile_data4 . $GLOBALS['PARAM_SEPARATOR'] . $profile_data5 . $GLOBALS['PARAM_SEPARATOR'] . $profile_autorun;
		}
		else
		{
			return "";			
		}
	 }
	 
	 //-------------------------------------------------------------
     //  InsertOrUpdateProfileData
     //-------------------------------------------------------------
     function InsertOrUpdateProfileData($iduser_par, $name_par, $address_par, $description_par, $data_par, $data2_par, $data3_par, $data4_par, $data5_par)
     {
		$query_consult_profile = "SELECT * FROM profile WHERE user = $iduser_par";
		$result_consult_profile = mysqli_query($GLOBALS['LINK_DATABASE'],$query_consult_profile) or die("Query Error::InsertOrUpdateProfileData::Select profile failed");
		
		if ($row_user_profile = mysqli_fetch_object($result_consult_profile))
		{
			if ((strlen($name_par) > 0) || (strlen($address_par) > 0) || (strlen($description_par) > 0))
			{
				// UPDATE "PROFILE" RECORD
				$query_update_profile = "UPDATE profile SET name='$name_par', address='$address_par', description='$description_par', data='$data_par', data2='$data2_par', data3='$data3_par', data4='$data4_par', data5='$data5_par' WHERE user = $iduser_par";
				mysqli_query($GLOBALS['LINK_DATABASE'],$query_update_profile) or die("Query Error::InsertOrUpdateProfileData::Update profile failed");

				if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
				{					
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		else
		{
			// ++ GET MAX ID ++
			$query_profile_consult = "SELECT max(id) as maximumId FROM profile";
			$result_profile_consult = mysqli_query($GLOBALS['LINK_DATABASE'],$query_profile_consult) or die("Query Error::InsertOrUpdateProfileData::Select max id profile failed");
			$row_profile_consult = mysqli_fetch_object($result_profile_consult);
			$maxIdentifier_profile = $row_profile_consult->maximumId + 1;
			mysqli_free_result($result_profile_consult);

			// INSERT A NEW RECORD IN "PROFILE" TABLE
			$query_profile_insert = "INSERT INTO profile VALUES ($maxIdentifier_profile, $iduser_par, '$name_par', '$address_par', '$description_par', '$data_par', '', '', '', '', 0)";	
			mysqli_query($GLOBALS['LINK_DATABASE'],$query_profile_insert) or die("Query Error::InsertOrUpdateProfileData::Insert profile failed");

			if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
			{					
				return true;
			}
			else
			{
				return false;
			}
		}
	 }
	 
	 //-------------------------------------------------------------
 	 //  ContainsString
     //-------------------------------------------------------------
	function ContainsString($needle, $haystack)
	{
		return strpos($haystack, $needle) !== false;
	}

	//-------------------------------------------------------------
	//  GetUserIPAddress
	//-------------------------------------------------------------
	function GetUserIPAddress() 
	{ 	
		// Check if IP is from shared internet
		if (!empty($_SERVER['HTTP_CLIENT_IP'])) {  
			$ip = $_SERVER['HTTP_CLIENT_IP'];  
		}  
		// Check if IP is passed from a proxy
		elseif (!empty($_SERVER['HTTP_X_FORWARDED_FOR'])) {  
			// Handle multiple IPs in HTTP_X_FORWARDED_FOR
			$ipList = explode(',', $_SERVER['HTTP_X_FORWARDED_FOR']);
			$ip = trim($ipList[0]); // Take the first IP in the list
		}  
		// Default to REMOTE_ADDR
		else {  
			$ip = $_SERVER['REMOTE_ADDR'];  
		}  
		return $ip;  
	}  
	
	 //-------------------------------------------------------------
 	 //  AllowIPToRegister
     //-------------------------------------------------------------
	 function AllowIPToRegister($ipadress_par, $email_par)
     {
		// Performing SQL Consult
		$query_data = "SELECT * FROM useripaddress WHERE address = '$ipadress_par' OR email = '$email_par'";
		$result_data = mysqli_query($GLOBALS['LINK_DATABASE'],$query_data) or die("Query Error::AllowIPToRegister=$ipadress_par");
		
		if ($row_data = mysqli_fetch_object($result_data))
		{
			$user_allowed = $row_data->allowed;
			$user_accounts = $row_data->accounts;
			if (($user_allowed == 1) && ($user_accounts == 0))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			return true;			
		}
	 }
	 
	 //-------------------------------------------------------------
 	 //  InsertIPAdress
     //-------------------------------------------------------------
	 function InsertIPAdress($ipadress_par, $email_par)
     {
		$query_consult_profile = "SELECT * FROM useripaddress WHERE address = '$ipadress_par' OR email = '$email_par'";
		$result_consult_profile = mysqli_query($GLOBALS['LINK_DATABASE'],$query_consult_profile) or die("Query Error::InsertIPAdress::Select ip address failed");
		
		if ($row_user_profile = mysqli_fetch_object($result_consult_profile))
		{
			// UPDATE "USERIPADDRESS" RECORD
			$id_ip_adress = $row_user_profile->id;
			$query_update_profile = "UPDATE useripaddress SET address = '$ipadress_par', email = '$email_par', accounts = 1 WHERE id = $id_ip_adress";
			mysqli_query($GLOBALS['LINK_DATABASE'],$query_update_profile) or die("Query Error::InsertIPAdress::Update ip adress failed");
			
			if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
			{					
				return true;
			}
			else
			{
				return false;
			}			
		}
		else
		{
			// ++ GET MAX ID ++
			$query_profile_consult = "SELECT max(id) as maximumId FROM useripaddress";
			$result_profile_consult = mysqli_query($GLOBALS['LINK_DATABASE'],$query_profile_consult) or die("Query Error::InsertIPAdress::Select max id useripaddress failed");
			$row_profile_consult = mysqli_fetch_object($result_profile_consult);
			$maxIdentifier_profile = $row_profile_consult->maximumId + 1;
			mysqli_free_result($result_profile_consult);		 
			
			// INSERT A NEW RECORD IN "USERIPADDRESS" TABLE
			$query_profile_insert = "INSERT INTO useripaddress VALUES ($maxIdentifier_profile, '$ipadress_par', 1, '$email_par', 1)";
			mysqli_query($GLOBALS['LINK_DATABASE'],$query_profile_insert) or die("Query Error::InsertIPAdress::Insert useripaddress failed");

			if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
			{					
				return true;
			}
			else
			{
				return false;
			}
		}
	 }
	
	 //-------------------------------------------------------------
 	 //  ResetIPAdresses
     //-------------------------------------------------------------
	 function ResetIPAdresses($ipadress_par, $email_par)
     {
		// UPDATE "USERIPADDRESS" RECORD
		$query_update_profile = "UPDATE useripaddress SET accounts = 0 WHERE address = '$ipadress_par' OR email = '$email_par'";
		mysqli_query($GLOBALS['LINK_DATABASE'],$query_update_profile) or die("Query Error::ResetIPAdresses::Update ip adress failed");
		
		if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
		{					
			return true;
		}
		else
		{
			return false;
		}		
	 }

	 //-------------------------------------------------------------
     //  RegisterProjectSlot
     //-------------------------------------------------------------
     function RegisterProjectSlot($user_par, $level_par, $timeout_par, $receipt_par)
     {
		$query_consult = "SELECT max(id) as maximumId FROM projectslots";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'],$query_consult) or die("Query Error::RegisterProjectSlot::Select max slots failed");
		$row_consult = mysqli_fetch_object($result_consult);
		$maxIdentifier = $row_consult->maximumId + 1;
		mysqli_free_result($result_consult);
		
		$query_insert = "INSERT INTO projectslots VALUES ($maxIdentifier, $user_par, -1, $level_par, $timeout_par, '$receipt_par')";	
		$result_insert = mysqli_query($GLOBALS['LINK_DATABASE'],$query_insert) or die("Query Error::UserRegisterByEmail::Insert users failed");
		
		if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
		{
			return $maxIdentifier;
		}
		else
		{
			return -1;
		}		
	 }

	 //-------------------------------------------------------------
     //  UpdateProjectSlot
     //-------------------------------------------------------------
     function UpdateProjectSlot($user_par, $slot_par, $project_par)
     {
		$query_string = "UPDATE projectslots SET project = ? WHERE id = ? AND user = ?";
		$query_update_data = mysqli_prepare($GLOBALS['LINK_DATABASE'], $query_string);
		mysqli_stmt_bind_param($query_update_data, 'iii', $project_par, $slot_par, $user_par);
		if (!mysqli_stmt_execute($query_update_data))
		{
			die("Query Error::UpdateProjectSlot::Update project slots Failed");
			return false;
		}
		else
		{
			return true;
		}
	 }

	 //-------------------------------------------------------------
     //  FreeProjectSlot
     //-------------------------------------------------------------
     function FreeProjectSlot($user_par, $project_par)
     {
		$query_update_slot = "UPDATE projectslots SET project = -1 WHERE user = $user_par AND project = $project_par";
		mysqli_query($GLOBALS['LINK_DATABASE'],$query_update_slot) or die("Query Error::FreeBookSlot::Update slot failed");
		
		if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
		{					
			return true;
		}
		else
		{
			return false;
		}		
	 }

	 //-------------------------------------------------------------
     //  DeleteProjectSlotsForUser
     //-------------------------------------------------------------
     function DeleteProjectSlotsForUser($user_par)
     {
		$query_delete_index = "DELETE FROM projectslots WHERE user = $user_par";
		mysqli_query($GLOBALS['LINK_DATABASE'], $query_delete_index) or die("Query Error::DeleteProjectSlotsForUser::Failed to delete projectslots $user_par");
	 }

	 //-------------------------------------------------------------
     //  DownloadCompletedReference
     //-------------------------------------------------------------
     function DownloadCompletedReference($id_par)
     {
		$query_consult = "SELECT * FROM projectsreference WHERE id = $id_par";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'], $query_consult) or die("Query Error::DownloadCompletedReference::List references failed");

		$size_previous = 0;
		$data = '';
		
		if ($row_data = mysqli_fetch_object($result_consult))
		{
			$data = $row_data->data;
		}
		mysqli_free_result($result_consult);
		
		return $data;
	  }	

     //-------------------------------------------------------------
     //  UploadStoryIndex
     //-------------------------------------------------------------
     function UploadStoryIndex($id_par, $user_par, $dataid_par, $title_par, $description_par, $category1_par, $category2_par, $category3_par, $timestamp_par, $should_print_par)
     {
		 $id_output = $id_par;
		 $dataid_output = -1;
		 
		 if (ExistsProjectIndex($id_par) == false)
		 {
			if ($id_output < 0) $id_output = 0;
		
			// New Index ID
			$query_maxindex = "SELECT max(id) as maximumId FROM projectsindex";
			$result_maxindex = mysqli_query($GLOBALS['LINK_DATABASE'],$query_maxindex) or die("Query Error::UploadStoryIndex::Select max projectsindex failed");
			$row_maxindex = mysqli_fetch_object($result_maxindex);
			$id_output = $row_maxindex->maximumId;
			if ($id_output == null) $id_output = 0;
			$id_output = $id_output + 1;
			mysqli_free_result($result_maxindex);

			$dataid_output = $id_output;

			$query_insert = "INSERT INTO projectsindex VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";
			$query_insert_story = mysqli_prepare($GLOBALS['LINK_DATABASE'], $query_insert);
			mysqli_stmt_bind_param($query_insert_story, 'iiissiiii', $id_output, $user_par, $dataid_output, $title_par, $description_par, $category1_par, $category2_par, $category3_par, $timestamp_par);
			if (!mysqli_stmt_execute($query_insert_story))
			{
				die("Query Error::UploadStoryIndex::Insert projectsindex Failed::$id_output, $user_par, $dataid_output, $title_par, $description_par, $category1_par, $category2_par, $category3_par, $timestamp_par");
			}
		 }
		 else
		 {
			// Consult Data ID
			$query_story = "SELECT data FROM projectsindex WHERE id = $id_par";
			$result_story = mysqli_query($GLOBALS['LINK_DATABASE'],$query_story) or die("Query Error::UploadStoryIndex::Exists Story Index");
			$dataid_output = $result_story->data;
					
			$query_string = "UPDATE projectsindex SET $title = ?, $description = ?, $category1 = ?, $category2 = ?, $category3 = ?, $time = ? WHERE id = ?";
			$query_update_story = mysqli_prepare($GLOBALS['LINK_DATABASE'], $query_string);
			mysqli_stmt_bind_param($query_update_story, 'ssiiiii', $title_par, $description_par, $category1_par, $category2_par, $category3_par, $timestamp, $id_par);
			if (!mysqli_stmt_execute($query_update_story))
			{
				die("Query Error::UploadStoryIndex::Update projectsindex Failed");
			}
		 }
		 
		 if ($should_print_par)
		 {
			print "true" . $GLOBALS['PARAM_SEPARATOR'] . $id_output . $GLOBALS['PARAM_SEPARATOR'] . $dataid_output;
		 }
		 
		 return $id_output;
    }

     //-------------------------------------------------------------
     //  UploadData
     //-------------------------------------------------------------
     function UploadData($user_par, $id_par, $data_par, $should_print_par)
     {
		 if (ExistsProjectData($user_par, $id_par) == false)
		 {
			$query_insert = "INSERT INTO projectsdata VALUES (?, ?, ?)";
			$query_insert_data = mysqli_prepare($GLOBALS['LINK_DATABASE'], $query_insert);
			mysqli_stmt_bind_param($query_insert_data, 'iis', $id_par, $user_par, $data_par);
			if (!mysqli_stmt_execute($query_insert_data))
			{
				die("Query Error::UploadData::Insert projectsdata Failed");
			}
		 }
		 else
		 {
			$query_string = "UPDATE projectsdata SET data = ? WHERE id = ? AND user = ?";
			$query_update_data = mysqli_prepare($GLOBALS['LINK_DATABASE'], $query_string);
			mysqli_stmt_bind_param($query_update_data, 'sii', $data_par, $id_par, $user_par);
			if (!mysqli_stmt_execute($query_update_data))
			{
				die("Query Error::UploadData::Update projectsdata Failed");
			}
		 }
		 
		 if ($should_print_par)
		 {
			print "true";
		 }
    }

	 //-------------------------------------------------------------
     //  InitAccountWithReferenceLevel
     //-------------------------------------------------------------
     function InitProjectWithReferenceLevel($user_par, $slot_par)
     {
		$should_print = false;
		
		// CREATE PROJECT INDEX RECORD
		$id = -1;
		$user = $user_par;
		$dataid = -1;
		$title = $GLOBALS['NAME_TEMPLATE_DEFAULT_PROJECT'];
		$description = $GLOBALS['DESCRIPTION_TEMPLATE_DEFAULT_PROJECT'];
		$category1 = "";
		$category2 = "";
		$category3 = "";
		$timestamp = 0;
		$id_new_project = UploadStoryIndex($id, $user, $dataid, $title, $description, $category1, $category2, $category3, $timestamp, $should_print);
		
		// GET DEFAULT LEVEL DATA
		$data_level = DownloadCompletedReference($GLOBALS['ID_TEMPLATE_DEFAULT_PROJECT']);
		
		// SAVE NEW PROJECT
		UploadData($user_par, $id_new_project, $data_level, $should_print);
		
		// UDPATE THE DEFAULT FREE SLOT
		UpdateProjectSlot($user_par, $slot_par, $id_new_project);
	 }	  
	
	 //-------------------------------------------------------------
 	 //  SendGlobalEmail
     //-------------------------------------------------------------
	 function SendGlobalEmail($email_from_par, $email_to_par, $content_subject_par, $content_body_par, $headers_par)
     {
		 /*
		$mail = new PHPMailer(true);                              // Passing `true` enables exceptions
		try {
			//Server settings
			$mail->SMTPDebug = 0;                                 // Enable verbose debug output
			$mail->isSMTP();                                      // Set mailer to use SMTP
			$mail->Host = 'smtp.dreamhost.com';                  // Specify main and backup SMTP servers
			$mail->SMTPAuth = true;                               // Enable SMTP authentication
			$mail->Username = 'info@workflowsimulator.com';             // SMTP username
			$mail->Password = 'password';                           // SMTP password
			$mail->SMTPSecure = 'tls';                            
			$mail->Port = 587;                                    // TCP port to connect to

			//Recipients
			$mail->setFrom('no-reply@workflowsimulator.com', 'Mailer');          //This is the email your form sends From
			$mail->addAddress($email_to_par);               // Name is optional

			//Content
			$mail->isHTML(true);                                  // Set email format to HTML
			$mail->Subject = $content_subject_par;
			$mail->Body    = $content_body_par;

			$mail->send();
		} catch (Exception $e) {
			return false;
		}
		 */
		return true;
	 }
?>