<?php
	
	include 'ConfigurationWorkflowSimulator.php';

	$user_id = $_POST["user"];
	$passw_user = $_POST["password"];
	$salt_user = $_POST["salt"];

	if (CheckValidUser($user_id, $passw_user, $salt_user))
	{
		$candidate = $_POST["candidate"];
		$analysis = $_POST["analysis"];
		
		MailAnalysis($candidate, $analysis);
		print "true";
	}
	else
	{
		print "false";
	}	

    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);
	
	//-------------------------------------------------------------
	//  MailAnalysis
	//-------------------------------------------------------------
	function MailAnalysis($candidate_par, $analysis_par)
	{
		// EMAIL LEVEL
		$to      = $GLOBALS['ANALYSIS_EMAIL_ADDRESS'];
		$subject = $GLOBALS['OFFICIAL_NAME_APPLICATION_GLOBAL'] . ' - Candidate Analysis';

		// EMAIL HTML MESSAGE
		$message = '<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Candidate Analysis</title>
  <meta name="description" content="" />

  <style>
    /* simplified for clarity */
    body { font-family: Arial, sans-serif; margin:0; padding:20px; }
    .card { padding:20px; border:1px solid #eee; border-radius:8px; }
    h1 { font-size:20px; }
  </style>
</head>
<body>
  <div class="card">
    <h1>'.htmlspecialchars($candidate_par).'</h1>
    <p>'.nl2br(htmlspecialchars($analysis_par)).'</p>
  </div>
</body>
</html>';

		// HEADERS
		$headers = "From: {$GLOBALS['NON_REPLY_EMAIL_ADDRESS']}\r\n";
		$headers .= "Reply-To: {$GLOBALS['NON_REPLY_EMAIL_ADDRESS']}\r\n";
		$headers .= "MIME-Version: 1.0\r\n";
		$headers .= "Content-Type: text/html; charset=UTF-8\r\n";

		SendGlobalEmail($GLOBALS['NON_REPLY_EMAIL_ADDRESS'], $to, $subject, $message, $headers);
	}


?>
