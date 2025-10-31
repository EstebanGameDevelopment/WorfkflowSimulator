<?php
	include 'ConfigurationWorkflowSimulator.php';
    
	$language = $_GET["language"];
	$emailuser = $_GET["email"];

	RequestResetByEmailPassword($language, $emailuser);

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
     //  MailPassword
     //-------------------------------------------------------------
	 function MailPassword($language_par, $email_real_par, $iduser_par, $code_par)
	 {
		 // EMAIL LEVEL
		 $to      = $email_real_par;
		$subject = 'Reset Your Password - ' . $GLOBALS['OFFICIAL_NAME_APPLICATION_GLOBAL'];

		$message = '
		<!DOCTYPE html>
		<html>
		<head>
			<meta charset="UTF-8">
			<title>Reset Your Password</title>
			<style>
				body {
					font-family: Arial, sans-serif;
					background-color: #f4f4f4;
					margin: 0;
					padding: 0;
				}
				.container {
					max-width: 600px;
					margin: 20px auto;
					background: #ffffff;
					padding: 20px;
					border-radius: 8px;
					box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
					text-align: center;
				}
				h1 {
					color: #d9534f;
				}
				p {
					font-size: 16px;
					color: #666666;
					line-height: 1.5;
				}
				.button {
					display: inline-block;
					margin-top: 20px;
					padding: 12px 20px;
					background: #007BFF;
					color: #ffffff;
					text-decoration: none;
					font-size: 16px;
					font-weight: bold;
					border-radius: 5px;
				}
				.footer {
					margin-top: 20px;
					font-size: 14px;
					color: #999999;
				}
			</style>
		</head>
		<body>
			<div class="container">
				<h1>Password Reset Request</h1>
				<p>We received a request to reset your password for <strong>' . htmlspecialchars($GLOBALS['OFFICIAL_NAME_APPLICATION_GLOBAL']) . '</strong>.</p>
				<p>If you did not request this, please ignore this email.</p>
				<p>Click the button below to reset your password:</p>
				<a href="' . htmlspecialchars($GLOBALS['URL_BASE_SERVER'] . 'FormPasswordReset.php?language=' . $language_par . '&id=' . $iduser_par . '&code=' . $code_par) . '" class="button">Reset My Password</a>
				<p>If the button does not work, copy and paste the following link into your browser:</p>
				<p><a href="' . htmlspecialchars($GLOBALS['URL_BASE_SERVER'] . 'FormPasswordReset.php?language=' . $language_par . '&id=' . $iduser_par . '&code=' . $code_par) . '">' . htmlspecialchars($GLOBALS['URL_BASE_SERVER'] . 'FormPasswordReset.php?language=' . $language_par . '&id=' . $iduser_par . '&code=' . $code_par) . '</a></p>
				<p>Best regards,</p>
				<p><strong>' . htmlspecialchars($GLOBALS['OFFICIAL_NAME_APPLICATION_GLOBAL']) . ' Team</strong></p>
				<div class="footer">
					<p>&copy; ' . date("Y") . ' ' . htmlspecialchars($GLOBALS['OFFICIAL_NAME_APPLICATION_GLOBAL']) . '. All rights reserved.</p>
				</div>
			</div>
		</body>
		</html>';
		 $headers = 'From: ' . $GLOBALS['NON_REPLY_EMAIL_ADDRESS'] . '\r\n' .
		 'Reply-To: ' . $GLOBALS['NON_REPLY_EMAIL_ADDRESS'] . '\r\n' .
		 'MIME-Version: 1.0\r\n' . '\r\n' .
		 'Content-Type: text/html; charset=ISO-8859-1\r\n';
		 
		 SendGlobalEmail($GLOBALS['NON_REPLY_EMAIL_ADDRESS'], $to, $subject, $message, $headers);
	 }		 

     //-------------------------------------------------------------
     //  RequestResetByEmailPassword
     //-------------------------------------------------------------
     function RequestResetByEmailPassword($language_par, $emailuser_par)
     {
		// Performing SQL Consult
		$query_user = "SELECT id FROM users WHERE email = '$emailuser_par'";
		$result_user = mysqli_query($GLOBALS['LINK_DATABASE'],$query_user) or die("Query Error::RequestResetByEmailPassword::Select users failed");
				
		if ($row_user = mysqli_fetch_object($result_user))
		{
			$id_user = $row_user->id;
			$random_code_reset = rand_string(6);
			
			// SET THE TIMESTAMP AND THE CODE TO RESET
			$query_update_user = "UPDATE users SET code='$random_code_reset' WHERE id = $id_user";
			$result_update_user = mysqli_query($GLOBALS['LINK_DATABASE'],$query_update_user) or die("Query Error::RequestResetByEmailPassword::Update users failed");

			if ($result_update_user)
			{
				MailPassword($language_par, $emailuser_par, $id_user, $random_code_reset);
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
	 
		// Free resultset
		mysqli_free_result($result_user);
    }
	
?>
