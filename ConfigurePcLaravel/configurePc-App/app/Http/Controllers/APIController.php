<?php

namespace App\Http\Controllers;
use App\Models\Order;
use Illuminate\Http\Request;

class APIController extends Controller
{
    public function sendCommand(Request $request)
    {
        // Validating incoming request data
        $validatedData = $request->validate([
            'command' => 'required|string',
            'parameters' => 'nullable|array',
        ]);

        // Processes the command and parameters
        $command = $validatedData['command'];
        $parameters = $validatedData['parameters'] ?? [];

        // Success response
        return response()->json([
            'message' => 'Command received successfully',
            'command' => $command,
            'parameters' => $parameters,
        ], 200);
    }
}