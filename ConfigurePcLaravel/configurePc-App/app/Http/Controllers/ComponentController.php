<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use App\Models\Component;

class ComponentController extends Controller
{
    public function index()
    {
        $components = Component::with(['category', 'specifications', 'wattageLists'])
            ->get();

        return response()->json($components);
    }
}
