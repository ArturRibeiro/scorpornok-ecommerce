import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Link } from "react-router";

export default function Recommendations() {
  return (
    <div className="mt-16">
      <Card>
        <CardHeader>
          <CardTitle>Você também pode gostar</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-center py-8">
            <p className="text-muted-foreground mb-4">
              Descubra mais produtos que combinam com você
            </p>
            <Button variant="outline" asChild>
              <Link to="/">Ver produtos</Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
