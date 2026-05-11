<?php

namespace Tests\Feature;

use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Http;
use Tests\TestCase;

class OrderEndpointTest extends TestCase
{
    use RefreshDatabase;

    public function test_orders_endpoint_requires_order_id(): void
    {
        $this->seed();

        $this->postJson('/api/orders', [
            'name' => 'Jane Doe',
            'email' => 'jane@example.com',
            'address' => 'Main Street 1',
            'trayIds' => [10, 11],
        ])->assertStatus(422)
            ->assertJsonValidationErrors(['id']);
    }

    public function test_orders_endpoint_forwards_upstream_failures(): void
    {
        $this->setProductionApiUrl('http://example.test');
        $this->seed();

        Http::fake([
            '*' => Http::response(['error' => 'warehouse unavailable'], 503),
        ]);

        $this->postJson('/api/orders', [
            'name' => 'Jane Doe',
            'email' => 'jane@example.com',
            'address' => 'Main Street 1',
            'id' => 42,
            'trayIds' => [10, 11],
        ])->assertStatus(503)
            ->assertJson(['error' => 'warehouse unavailable']);
    }

    public function test_command_endpoint_requires_parameters_to_be_an_array_when_present(): void
    {
        $this->postJson('/api/production/command', [
            'command' => 'start',
            'parameters' => 'fast',
        ])->assertStatus(422)
            ->assertJsonValidationErrors(['parameters']);
    }

    private function setProductionApiUrl(string $url): void
    {
        putenv("PRODUCTION_API_URL={$url}");
        $_ENV['PRODUCTION_API_URL'] = $url;
        $_SERVER['PRODUCTION_API_URL'] = $url;
    }
}
