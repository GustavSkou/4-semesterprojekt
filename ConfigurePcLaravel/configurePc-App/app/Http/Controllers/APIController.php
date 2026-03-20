<?php

namespace App\Http\Controllers;
use App\Models\Order;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;


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

    public function sendOrder(Request $request) {
        $validated = $request->validate([
            'id' => 'required|integer',
            'trayIds' => 'required|array',
            'trayIds.*' => 'integer',
        ]);

        $url = env('PRODUCTION_API_URL') . '/ProductionSystem/Command';

        $response = Http::post($url, [
            'Name' => 'order',
            'Parameters' => [
                    'id' => (string) $validated['id'],
                    'trayIds' => implode(',', $validated['trayIds']),
            ]
        ]);

        return response()->json($response->json(), $response->status());

    }
}