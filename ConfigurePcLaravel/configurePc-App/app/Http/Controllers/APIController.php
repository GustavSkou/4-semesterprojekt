<?php

namespace App\Http\Controllers;
use App\Models\Order;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

class APIController extends Controller
{
    public function sendCommand(Request $request)
    {
        $validatedData = $request->validate([
            'command' => 'required|string',
            'parameters' => 'nullable|array',
        ]);

        $command = $validatedData['command'];
        $parameters = $validatedData['parameters'] ?? [];


        $response = Http::post(env('PRODUCTION_API_URL') . '/ProductionSystem/Command', [
            'Name' => $command,
            'Parameters' => $parameters,
        ]);

        return response()->json([
            'message' => 'Command sent successfully',
            'response' => $response->json(),
        ], $response->status());
    }
}