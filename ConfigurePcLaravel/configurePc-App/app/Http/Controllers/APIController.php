<?php

namespace App\Http\Controllers;
use App\Models\Computer;
use App\Models\Component;
use App\Models\Customer;
use App\Models\Order;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;
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
            'name' => 'required|string|max:255',
            'email' => 'required|email|max:255',
            'address' => 'required|string|max:255',
            'id' => 'required|integer',
            'trayIds' => 'required|array',
            'trayIds.*' => 'integer|exists:components,tray_id',
        ]);

        $components = Component::whereIn('tray_id', $validated['trayIds'])->get()->keyBy('tray_id');

        if ($components->count() !== count($validated['trayIds'])) {
            return response()->json([
                'message' => 'One or more selected components are out of stock or unavailable.',
                'errors' => [
                    'trayIds' => ['One or more selected components are out of stock or unavailable.'],
                ],
            ], 422);
        }

        DB::transaction(function () use ($validated, $components) {
            $customer = Customer::firstOrCreate(
                ['email' => $validated['email']],
                [
                    'name' => $validated['name'],
                    'address' => $validated['address'],
                ]
            );

            $customer->update([
                'name' => $validated['name'],
                'address' => $validated['address'],
            ]);

            $order = Order::create([
                'order_date' => now()->toDateString(),
                'status' => 'submitted',
                'customer_id' => $customer->id,
            ]);

            $computer = Computer::create([
                'order_id' => $order->id,
            ]);

            $computer->components()->attach(
                collect($validated['trayIds'])
                    ->map(fn (int $trayId) => $components[$trayId]->id)
                    ->all()
            );
        });

        $url = env('PRODUCTION_API_URL') . '/ProductionSystem/Command';

        $response = Http::post($url, [
            'Name' => 'order',
            'Parameters' => [
                'id'    => $validated['id'],
                'items' => $validated['trayIds'],
            ]
        ]);

        return response()->json($response->json(), $response->status());

    }
}
